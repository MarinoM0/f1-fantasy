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
            var teamName = request.Name.Trim();

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

            var existingTeam = await _dbContext.FantasyTeams
                .AsNoTracking()
                .AnyAsync(x => x.UserId == userId);

            if (existingTeam)
            {
                throw new InvalidOperationException("User already has a fantasy team.");
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

            var fantasyTeam = new FantasyTeam
            {
                Name = teamName,
                BudgetCap = BudgetCap,
                RemainingBudget = BudgetCap - totalPrice,
                UserId = userId,
                FantasyTeamDrivers = distinctDriverIds
                    .Select(driverId => new FantasyTeamDriver
                    {
                        DriverId = driverId
                    })
                    .ToList(),
                FantasyTeamConstructors = distinctConstructorIds
                    .Select(constructorId => new FantasyTeamConstructor
                    {
                        ConstructorId = constructorId
                    })
                    .ToList()
            };

            _dbContext.FantasyTeams.Add(fantasyTeam);
            await _dbContext.SaveChangesAsync();

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
