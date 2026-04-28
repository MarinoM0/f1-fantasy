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

        public FantasyTeamService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
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
                .Include(x => x.FantasyTeamConstructors)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (fantasyTeam is null)
            {
                throw new KeyNotFoundException("Fantasy team not found.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            _dbContext.FantasyTeams.Remove(fantasyTeam);
            await _dbContext.SaveChangesAsync();

            var replacementTeam = CreateFantasyTeamEntity(userId, selection);
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
                    UserId = x.UserId,
                    Username = x.User.Username,
                    Constructors = x.FantasyTeamConstructors
                        .OrderBy(tc => tc.Constructor.Name)
                        .Select(tc => new FantasyTeamConstructorDto
                        {
                            Id = tc.Constructor.Id,
                            Name = tc.Constructor.Name,
                            Code = tc.Constructor.Code,
                            Price = tc.Constructor.Price
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
                            ConstructorCode = td.Driver.Constructor.Code
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
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
    }
}
