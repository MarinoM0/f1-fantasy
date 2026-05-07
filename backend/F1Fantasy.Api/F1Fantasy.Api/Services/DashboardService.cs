using F1Fantasy.Api.Data;
using F1Fantasy.Api.DTOs.Dashboard;
using F1Fantasy.Api.DTOs.Jolpica;
using F1Fantasy.Api.Interfaces;
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

            var driverPointsByCode = LivePointsCalculatorService.BuildDriverPointsByCode(driverStandings);
            var constructorPointsByJolpicaId = LivePointsCalculatorService.BuildConstructorPointsByJolpicaId(constructorStandings);

            var hasLiveStandings = driverPointsByCode.Count > 0 || constructorPointsByJolpicaId.Count > 0;

            var leaderboard = new List<DashboardLeaderboardEntryDto>();

            foreach (var team in teams)
            {
                var totalPoints = hasLiveStandings
                    ? LivePointsCalculatorService.CalculateTeamPoints(team, driverPointsByCode, constructorPointsByJolpicaId)
                    : team.User.TeamScores.Sum(score => score.Points);

                leaderboard.Add(new DashboardLeaderboardEntryDto
                {
                    Rank = 0,
                    UserId = team.UserId,
                    Username = team.User.Username,
                    TeamName = team.Name,
                    TotalPoints = totalPoints
                });
            }

            var rankedLeaderbord = leaderboard
                .OrderByDescending(entry => entry.TotalPoints)
                .ThenBy(entry => entry.Username)
                .Take(50)
                .ToList();

            for (var i = 0; i < rankedLeaderbord.Count; i++)
            {
                rankedLeaderbord[i].Rank = i + 1;
            }

            return rankedLeaderbord;
        }
        }
    }
