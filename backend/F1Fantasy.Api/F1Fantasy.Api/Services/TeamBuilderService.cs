using F1Fantasy.Api.Data;
using F1Fantasy.Api.DTOs.TeamBuilder;
using F1Fantasy.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace F1Fantasy.Api.Services
{
    public class TeamBuilderService
    {
        private readonly AppDbContext _dbContext;
        private readonly IJolpicaService _jolpicaService;

        public TeamBuilderService(AppDbContext dbContext, IJolpicaService jolpicaService)
        {
            _dbContext = dbContext;
            _jolpicaService = jolpicaService;
        }

        public async Task<IReadOnlyList<TeamBuilderDriverDto>> GetDriversAsync(CancellationToken cancellationToken = default)
        {
            var localDrivers = await _dbContext.Drivers
                .AsNoTracking()
                .Include(d => d.Constructor)
                .OrderBy(d => d.Price)
                .ThenBy(d => d.LastName)
                .ThenBy(d => d.FirstName)
                .ToListAsync(cancellationToken);

            var liveStandings = await _jolpicaService.GetDriverStandingsAsync(cancellationToken: cancellationToken);

            var liveByCode = liveStandings
                .Where(x => !string.IsNullOrWhiteSpace(x.Code))
                .GroupBy(x => x.Code.Trim().ToUpperInvariant())
                .ToDictionary(g => g.Key, g => g.First());

            return localDrivers.Select(driver =>
            {
                var code = driver.Code.Trim().ToUpperInvariant();
                var hasLiveData = liveByCode.TryGetValue(code, out var live);

                return new TeamBuilderDriverDto
                {
                    Id = driver.Id,
                    FirstName = driver.FirstName,
                    LastName = driver.LastName,
                    FullName = $"{driver.FirstName} {driver.LastName}",
                    Code = driver.Code,
                    Price = driver.Price,
                    ConstructorId = driver.ConstructorId,
                    ConstructorName = driver.Constructor.Name,
                    ConstructorCode = driver.Constructor.Code,
                    StandingPosition = hasLiveData ? live!.Position : null,
                    CurrentPoints = hasLiveData ? live!.Points : 0m,
                    CurrentWins = hasLiveData ? live!.Wins : 0,
                    HasLiveData = hasLiveData
                };
            }).ToList();
        }

        public async Task<IReadOnlyList<TeamBuilderConstructorDto>> GetConstructorsAsync(CancellationToken cancellationToken = default)
        {
            var localConstructors = await _dbContext.Constructors
                .AsNoTracking()
                .OrderBy(c => c.Price)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);

            var liveStandings = await _jolpicaService.GetConstructorStandingsAsync(cancellationToken: cancellationToken);

            var liveByKey = liveStandings
                .Select(x => new
                {
                    Key = NormalizeConstructorName(x.ConstructorName),
                    Value = x
                })
                .GroupBy(x => x.Key)
                .ToDictionary(g => g.Key, g => g.First().Value);

            return localConstructors.Select(constructor =>
            {
                var possibleKeys = GetConstructorMatchKeys(constructor);
                var live = possibleKeys
                    .Select(key => liveByKey.TryGetValue(key, out var match) ? match : null)
                    .FirstOrDefault(x => x is not null);

                var hasLiveData = live is not null;

                return new TeamBuilderConstructorDto
                {
                    Id = constructor.Id,
                    Name = constructor.Name,
                    Code = constructor.Code,
                    Price = constructor.Price,
                    StandingPosition = hasLiveData ? live!.Position : null,
                    CurrentPoints = hasLiveData ? live!.Points : 0m,
                    CurrentWins = hasLiveData ? live!.Wins : 0,
                    HasLiveData = hasLiveData
                };
            }).ToList();
        }

        private static string NormalizeConstructorName(string value)
        {
            return value
                .Trim()
                .ToUpperInvariant()
                .Replace("RACING", "")
                .Replace("TEAM", "")
                .Replace("F1", "")
                .Replace("FORMULA 1", "")
                .Replace("-", " ")
                .Replace("_", " ")
                .Replace(".", "")
                .Replace("  ", " ")
                .Trim();
        }

        private static IReadOnlyList<string> GetConstructorMatchKeys(Models.Constructor constructor)
        {
            var keys = new List<string>
            {
                NormalizeConstructorName(constructor.Name)
            };

            switch (constructor.Code.Trim().ToUpperInvariant())
            {
                case "RBR":
                    keys.Add(NormalizeConstructorName("Red Bull"));
                    keys.Add(NormalizeConstructorName("Red Bull Racing"));
                    break;

                case "HAA":
                    keys.Add(NormalizeConstructorName("Haas"));
                    keys.Add(NormalizeConstructorName("Haas F1 Team"));
                    break;

                case "RBT":
                    keys.Add(NormalizeConstructorName("RB"));
                    keys.Add(NormalizeConstructorName("Racing Bulls"));
                    break;

                case "AMR":
                    keys.Add(NormalizeConstructorName("Aston Martin"));
                    keys.Add(NormalizeConstructorName("Aston Martin Aramco"));
                    break;

                case "AUD":
                    keys.Add(NormalizeConstructorName("Audi"));
                    keys.Add(NormalizeConstructorName("Sauber"));
                    break;
            }

            return keys.Distinct().ToList();
        }
    }
}
