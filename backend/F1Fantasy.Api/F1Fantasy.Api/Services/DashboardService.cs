using F1Fantasy.Api.Data;
using F1Fantasy.Api.DTOs.Dashboard;
using F1Fantasy.Api.DTOs.Jolpica;
using F1Fantasy.Api.Interfaces;
using F1Fantasy.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace F1Fantasy.Api.Services
{
    public class DashboardService
    {
        private readonly AppDbContext _dbContext;
        private readonly IJolpicaService _jolpicaService;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            AppDbContext dbContext,
            IJolpicaService jolpicaService,
            ILogger<DashboardService> logger)
        {
            _dbContext = dbContext;
            _jolpicaService = jolpicaService;
            _logger = logger;
        }

        public async Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
        {
            var upcomingRace = await _jolpicaService.GetNextRaceAsync(cancellationToken: cancellationToken)
                ?? await GetNextRaceFromDatabaseAsync(cancellationToken);

            var leaderboard = await GetLeaderboardAsync(cancellationToken);

            return new DashboardDto
            {
                UpcomingRace = upcomingRace,
                Leaderboard = leaderboard
            };
        }

        private async Task<DashboardUpcomingRaceDto?> GetNextRaceFromDatabaseAsync(CancellationToken cancellationToken)
        {
            var nowUtc = DateTime.UtcNow;

            return await _dbContext.Races
                .AsNoTracking()
                .Where(r => !r.IsCompleted && r.StartTimeUtc >= nowUtc)
                .OrderBy(r => r.StartTimeUtc)
                .Select(r => new DashboardUpcomingRaceDto
                {
                    LocalRaceId = r.Id,
                    Season = r.StartTimeUtc.Year.ToString(),
                    RoundNumber = r.RoundNumber,
                    RaceName = r.Name,
                    CircuitId = string.Empty,
                    CircuitName = r.CircuitName,
                    Country = r.Country,
                    Locality = string.Empty,
                    StartTimeUtc = r.StartTimeUtc,
                    DataSource = "local"
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<IReadOnlyList<DashboardLeaderboardEntryDto>> GetLeaderboardAsync(
            CancellationToken cancellationToken)
        {
            var teams = await _dbContext.FantasyTeams
                .AsNoTracking()
                .Include(team => team.User)
                    .ThenInclude(user => user.TeamScores)
                .Include(team => team.FantasyTeamDrivers)
                    .ThenInclude(teamDriver => teamDriver.Driver)
                .Include(team => team.FantasyTeamConstructors)
                    .ThenInclude(teamConstructor => teamConstructor.Constructor)
                .ToListAsync(cancellationToken);

            var driverStandings = Array.Empty<JolpicaDriverStandingDto>() as IReadOnlyList<JolpicaDriverStandingDto>;
            var constructorStandings = Array.Empty<JolpicaConstructorStandingDto>() as IReadOnlyList<JolpicaConstructorStandingDto>;

            try
            {
                var driverStandingsTask = _jolpicaService.GetDriverStandingsAsync(cancellationToken: cancellationToken);
                var constructorStandingsTask = _jolpicaService.GetConstructorStandingsAsync(cancellationToken: cancellationToken);

                await Task.WhenAll(driverStandingsTask, constructorStandingsTask);

                driverStandings = await driverStandingsTask;
                constructorStandings = await constructorStandingsTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch live standings for dashboard leaderboard");
            }

            var driverPointsByCode = driverStandings
                .Where(standing => !string.IsNullOrWhiteSpace(standing.Code))
                .GroupBy(standing => NormalizeKey(standing.Code))
                .ToDictionary(group => group.Key, group => group.First().Points);

            var constructorPointsByKey = constructorStandings
                .Where(standing => !string.IsNullOrWhiteSpace(standing.ConstructorName))
                .GroupBy(standing => NormalizeConstructorName(standing.ConstructorName))
                .ToDictionary(group => group.Key, group => group.First().Points);

            var hasLiveStandings = driverPointsByCode.Count > 0 || constructorPointsByKey.Count > 0;

            var leaderboard = teams
                .Select(team => new DashboardLeaderboardEntryDto
                {
                    Rank = 0,
                    UserId = team.UserId,
                    Username = team.User.Username,
                    TeamName = team.Name,
                    TotalPoints = hasLiveStandings
                        ? CalculateLiveTeamPoints(team, driverPointsByCode, constructorPointsByKey)
                        : team.User.TeamScores.Sum(score => score.Points)
                })
                .OrderByDescending(entry => entry.TotalPoints)
                .ThenBy(entry => entry.Username)
                .Take(50)
                .ToList();

            for (var i = 0; i < leaderboard.Count; i++)
            {
                leaderboard[i].Rank = i + 1;
            }

            return leaderboard;
        }

        private static decimal CalculateLiveTeamPoints(
            FantasyTeam team,
            IReadOnlyDictionary<string, decimal> driverPointsByCode,
            IReadOnlyDictionary<string, decimal> constructorPointsByKey)
        {
            var driverPoints = team.FantasyTeamDrivers.Sum(teamDriver =>
            {
                var code = NormalizeKey(teamDriver.Driver.Code);

                return driverPointsByCode.TryGetValue(code, out var points)
                    ? points
                    : 0m;
            });

            var constructorPoints = team.FantasyTeamConstructors.Sum(teamConstructor =>
                GetConstructorMatchKeys(teamConstructor.Constructor)
                    .Select(key => constructorPointsByKey.TryGetValue(key, out var points)
                        ? points
                        : (decimal?)null)
                    .FirstOrDefault(points => points.HasValue) ?? 0m);

            return driverPoints + constructorPoints;
        }

        private static string NormalizeKey(string value)
            => value.Trim().ToUpperInvariant();

        private static string NormalizeConstructorName(string value)
        {
            return NormalizeKey(value)
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

        private static IReadOnlyList<string> GetConstructorMatchKeys(Constructor constructor)
        {
            var keys = new List<string>
            {
                NormalizeConstructorName(constructor.Name)
            };

            switch (NormalizeKey(constructor.Code))
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
