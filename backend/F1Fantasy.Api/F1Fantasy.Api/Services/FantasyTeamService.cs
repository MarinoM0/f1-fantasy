using F1Fantasy.Api.Data;
using F1Fantasy.Api.Dtos;
using F1Fantasy.Api.DTOs;
using F1Fantasy.Api.Interfaces;
using F1Fantasy.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace F1Fantasy.Api.Services
{
    public class FantasyTeamService : IFantasyTeamService
    {
        private const decimal BudgetCap = 100.00m;

        private readonly AppDbContext _dbContext;
        private readonly IFantasyScoringService _fantasyScoringService;
        private readonly ILogger<FantasyTeamService> _logger;

        public FantasyTeamService(
            AppDbContext dbContext,
            IFantasyScoringService fantasyScoringService,
            ILogger<FantasyTeamService> logger)
        {
            _dbContext = dbContext;
            _fantasyScoringService = fantasyScoringService;
            _logger = logger;
        }

        public async Task<FantasyTeamDto> CreateAsync(int userId, CreateFantasyTeamRequestDto request)
        {
            var selection = await ValidateTeamSelectionAsync(request);

            var existingTeam = await _dbContext.FantasyTeams
                .AsNoTracking()
                .AnyAsync(x => x.UserId == userId);

            if (existingTeam)
            {
                throw new InvalidOperationException("User already has a fantasy team.");
            }

            var fantasyTeam = CreateFantasyTeamEntity(userId, selection);

            _dbContext.FantasyTeams.Add(fantasyTeam);
            await _dbContext.SaveChangesAsync();

            return await GetMyTeamOrThrowAsync(userId);
        }

        public async Task<FantasyTeamDto> UpdateAsync(int userId, CreateFantasyTeamRequestDto request)
        {
            var selection = await ValidateTeamSelectionAsync(request);

            var fantasyTeam = await _dbContext.FantasyTeams
                .Include(x => x.FantasyTeamDrivers)
                    .ThenInclude(td => td.Driver)
                .Include(x => x.FantasyTeamConstructors)
                    .ThenInclude(tc => tc.Constructor)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (fantasyTeam is null)
            {
                throw new KeyNotFoundException("Fantasy team not found.");
            }

            if (fantasyTeam.HasUsedTransfer)
            {
                throw new InvalidOperationException("You have already used your one allowed team transfer");
            }

            var currentPoints = await BuildCurrentPointsAsync();

            var newLockedInPoints = LivePointsCalculatorService.CalculateTeamPoints(
                fantasyTeam,
                currentPoints.DriverPointsByCode,
                currentPoints.ConstructorPointsByJolpicaId);

            var driverBaselines = await BuildDriverBaselinesAsync(
                selection.DriverIds, currentPoints.DriverPointsByCode);

            var constructorBaselines = await BuildConstructorBaselinesAsync(
                selection.ConstructorIds, currentPoints.ConstructorPointsByJolpicaId);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            _dbContext.FantasyTeams.Remove(fantasyTeam);
            await _dbContext.SaveChangesAsync();

            var replacementTeam = CreateReplacementTeamEntity(
                userId, selection, newLockedInPoints, driverBaselines, constructorBaselines);
            _dbContext.FantasyTeams.Add(replacementTeam);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return await GetMyTeamOrThrowAsync(userId);
        }

        public async Task<FantasyTeamDto?> GetMyTeamAsync(int userId)
        {
            return await _dbContext.FantasyTeams
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => new FantasyTeamDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    BudgetCap = x.BudgetCap,
                    RemainingBudget = x.RemainingBudget,
                    HasUsedTransfer = x.HasUsedTransfer,
                    LockedInPoints = x.LockedInPoints,
                    UserId = x.UserId,
                    Username = x.User.Username,
                    Constructors = x.FantasyTeamConstructors
                        .OrderBy(tc => tc.Constructor.Name)
                        .Select(tc => new FantasyTeamConstructorDto
                        {
                            Id = tc.Constructor.Id,
                            Name = tc.Constructor.Name,
                            Code = tc.Constructor.Code,
                            Price = tc.Constructor.Price,
                            PointsAtTransfer = tc.PointsAtTransfer
                        })
                        .ToList(),
                    Drivers = x.FantasyTeamDrivers
                        .OrderBy(td => td.Driver.LastName)
                        .ThenBy(td => td.Driver.FirstName)
                        .Select(td => new FantasyTeamDriverDto
                        {
                            Id = td.Driver.Id,
                            FirstName = td.Driver.FirstName,
                            LastName = td.Driver.LastName,
                            FullName = td.Driver.FirstName + " " + td.Driver.LastName,
                            Code = td.Driver.Code,
                            Price = td.Driver.Price,
                            ConstructorId = td.Driver.ConstructorId,
                            ConstructorName = td.Driver.Constructor.Name,
                            ConstructorCode = td.Driver.Constructor.Code,
                            PointsAtTransfer = td.PointsAtTransfer
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        // Snapshot of each driver's / constructor's accumulated fantasy points so
        // far. Used to lock in the outgoing team's earned points and to baseline
        // the incoming picks on transfer.
        private async Task<CurrentPoints> BuildCurrentPointsAsync()
        {
            var driverPoints = await _fantasyScoringService.BuildDriverPointsByCodeAsync();
            var constructorPoints = await _fantasyScoringService.BuildConstructorPointsByJolpicaIdAsync();

            return new CurrentPoints(driverPoints, constructorPoints);
        }

  
        private async Task<IReadOnlyDictionary<int, decimal>> BuildDriverBaselinesAsync(
            IReadOnlyCollection<int> driverIds,
            IReadOnlyDictionary<string, decimal> driverPointsByCode)
        {
            var drivers = await _dbContext.Drivers
                .Where(d => driverIds.Contains(d.Id))
                .ToListAsync();

            var baselines = new Dictionary<int, decimal>();

            foreach (var driver in drivers)
            {
                var currentPoints = LivePointsCalculatorService.GetDriverPoints(driver, driverPointsByCode);
                baselines[driver.Id] = currentPoints;
            }

            return baselines;
        }


        private async Task<IReadOnlyDictionary<int, decimal>> BuildConstructorBaselinesAsync(
            IReadOnlyCollection<int> constructorIds,
            IReadOnlyDictionary<string, decimal> constructorPointsByJolpicaId)
        {
            var constructors = await _dbContext.Constructors
                .Where(c => constructorIds.Contains(c.Id))
                .ToListAsync();

            var baselines = new Dictionary<int, decimal>();

            foreach (var constructor in constructors)
            {
                var currentPoints = LivePointsCalculatorService.GetConstructorPoints(
                    constructor, constructorPointsByJolpicaId);
                baselines[constructor.Id] = currentPoints;
            }

            return baselines;
        }


        private static FantasyTeam CreateFantasyTeamEntity(int userId, ValidatedTeamSelection selection)
        {
            return new FantasyTeam
            {
                Name = selection.TeamName,
                BudgetCap = BudgetCap,
                RemainingBudget = BudgetCap - selection.TotalPrice,
                UserId = userId,
                FantasyTeamDrivers = selection.DriverIds
                    .Select(driverId => new FantasyTeamDriver
                    {
                        DriverId = driverId
                    })
                    .ToList(),
                FantasyTeamConstructors = selection.ConstructorIds
                    .Select(constructorId => new FantasyTeamConstructor
                    {
                        ConstructorId = constructorId
                    })
                    .ToList()
            };
        }

        private static FantasyTeam CreateReplacementTeamEntity(
            int userId,
            ValidatedTeamSelection selection,
            decimal lockedInPoints,
            IReadOnlyDictionary<int, decimal> driverBaselines,
            IReadOnlyDictionary<int, decimal> constructorBaselines)
        {
            var teamDrivers = new List<FantasyTeamDriver>();

            foreach (var driverId in selection.DriverIds)
            {
                var baseline = driverBaselines.TryGetValue(driverId, out var points) ? points : 0m;

                teamDrivers.Add(new FantasyTeamDriver
                {
                    DriverId = driverId,
                    PointsAtTransfer = baseline
                });
            }

            var teamConstructors = new List<FantasyTeamConstructor>();

            foreach (var constructorId in selection.ConstructorIds)
            {
                var baseline = constructorBaselines.TryGetValue(constructorId, out var points) ? points : 0m;

                teamConstructors.Add(new FantasyTeamConstructor
                {
                    ConstructorId = constructorId,
                    PointsAtTransfer = baseline
                });
            }

            return new FantasyTeam
            {
                Name = selection.TeamName,
                BudgetCap = BudgetCap,
                RemainingBudget = BudgetCap - selection.TotalPrice,
                UserId = userId,
                HasUsedTransfer = true,
                LockedInPoints = lockedInPoints,
                FantasyTeamDrivers = teamDrivers,
                FantasyTeamConstructors = teamConstructors
            };
        }

        private async Task<ValidatedTeamSelection> ValidateTeamSelectionAsync(CreateFantasyTeamRequestDto request)
        {
            var teamName = request.Name?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(teamName))
            {
                throw new ArgumentException("Team name is required.");
            }

            if (request.DriverIds.Count != 5)
            {
                throw new ArgumentException("You must select exactly 5 drivers.");
            }

            if (request.ConstructorIds.Count != 2)
            {
                throw new ArgumentException("You must select exactly 2 constructors.");
            }

            var distinctDriverIds = request.DriverIds.Distinct().ToList();
            if (distinctDriverIds.Count != 5)
            {
                throw new ArgumentException("Drivers must be unique.");
            }

            var distinctConstructorIds = request.ConstructorIds.Distinct().ToList();
            if (distinctConstructorIds.Count != 2)
            {
                throw new ArgumentException("Constructors must be unique.");
            }

            var constructors = await _dbContext.Constructors
                .Where(x => distinctConstructorIds.Contains(x.Id))
                .ToListAsync();

            if (constructors.Count != 2)
            {
                throw new ArgumentException("One or more selected constructors do not exist.");
            }

            var drivers = await _dbContext.Drivers
                .Where(x => distinctDriverIds.Contains(x.Id))
                .ToListAsync();

            if (drivers.Count != 5)
            {
                throw new ArgumentException("One or more selected drivers do not exist.");
            }

            var totalPrice = drivers.Sum(x => x.Price) + constructors.Sum(x => x.Price);

            if (totalPrice > BudgetCap)
            {
                throw new InvalidOperationException("Selected team exceeds the budget cap.");
            }

            return new ValidatedTeamSelection(teamName, distinctDriverIds, distinctConstructorIds, totalPrice);
        }

        private async Task<FantasyTeamDto> GetMyTeamOrThrowAsync(int userId)
        {
            var team = await GetMyTeamAsync(userId);

            if (team is null)
            {
                throw new InvalidOperationException("Fantasy team could not be loaded.");
            }

            return team;
        }

        private sealed record ValidatedTeamSelection(
            string TeamName,
            IReadOnlyCollection<int> DriverIds,
            IReadOnlyCollection<int> ConstructorIds,
            decimal TotalPrice);

        private sealed record CurrentPoints(
            IReadOnlyDictionary<string, decimal> DriverPointsByCode,
            IReadOnlyDictionary<string, decimal> ConstructorPointsByJolpicaId);
    }
}
