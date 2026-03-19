using F1Fantasy.Api.Data;
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
            var teamName = request.Name.Trim();

            if(String.IsNullOrWhiteSpace(teamName))
            {
                throw new ArgumentException("Team name is required");
            }

            if (request.DriverIds.Count != 5)
            {
                throw new ArgumentException("You must select exactly 5 drivers.");
            }

            var distinctDriverIds = request.DriverIds.Distinct().ToList();

            if (distinctDriverIds.Count != 5)
            {
                throw new ArgumentException("Drivers must be unique.");
            }

            var existingTeam = await _dbContext.FantasyTeams
                .AsNoTracking()
                .AnyAsync(x => x.UserId == userId);

            if (existingTeam)
            {
                throw new InvalidOperationException("User already has a fantasy team.");
            }

            var constructor = await _dbContext.Constructors
            .FirstOrDefaultAsync(x => x.Id == request.ConstructorId);

            if (constructor is null)
            {
                throw new ArgumentException("Selected constructor does not exist.");
            }

            var drivers = await _dbContext.Drivers
                .Where(x => distinctDriverIds.Contains(x.Id))
                .ToListAsync();

            if (drivers.Count != 5)
            {
                throw new ArgumentException("One or more selected drivers do not exist.");
            }

            var totalDriverPrice = drivers.Sum(x => x.Price);
            var totalPrice = totalDriverPrice + constructor.Price;

            if (totalPrice > BudgetCap)
            {
                throw new InvalidOperationException("Selected team exceeds the budget cap.");
            }

            var fantasyTeam = new FantasyTeam
            {
                Name = teamName,
                BudgetCap = BudgetCap,
                RemainingBudget = BudgetCap - totalPrice,
                UserId = userId,
                ConstructorId = constructor.Id,
                FantasyTeamDrivers = distinctDriverIds
                    .Select(driverId => new FantasyTeamDriver
                    {
                        DriverId = driverId
                    })
                    .ToList()
            };

            _dbContext.FantasyTeams.Add(fantasyTeam);
            await _dbContext.SaveChangesAsync();

            return await GetMyTeamOrThrowAsync(userId);
        }

        public async Task<FantasyTeamDto?> GetMyTeamAsync (int userId)
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
                    ConstructorId = x.ConstructorId,
                    ConstructorName = x.Constructor.Name,
                    ConstructorCode = x.Constructor.Code,
                    ConstructorPrice = x.Constructor.Price,
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

        private async Task<FantasyTeamDto> GetMyTeamOrThrowAsync(int userId)
        {
            var team = await GetMyTeamAsync(userId);

            if (team is null)
            {
                throw new InvalidOperationException("Fantasy team was created but could not be loaded.");
            }

            return team;
        }
    }
}
