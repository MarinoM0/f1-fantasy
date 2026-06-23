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
        private readonly IFantasyScoringService _fantasyScoringService;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            AppDbContext dbContext,
            IJolpicaService jolpicaService,
            IFantasyScoringService fantasyScoringService,
            ILogger<DashboardService> logger)
        {
            _dbContext = dbContext;
            _jolpicaService = jolpicaService;
            _fantasyScoringService = fantasyScoringService;
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

            // Team totals are driven by accumulated fantasy points from synced
            // race results (not live championship standings).
            var driverPointsByCode = await _fantasyScoringService
                .BuildDriverPointsByCodeAsync(cancellationToken);
            var constructorPointsByJolpicaId = await _fantasyScoringService
                .BuildConstructorPointsByJolpicaIdAsync(cancellationToken);

            var leaderboard = new List<DashboardLeaderboardEntryDto>();

            foreach (var team in teams)
            {
                var totalPoints = LivePointsCalculatorService.CalculateTeamPoints(
                    team, driverPointsByCode, constructorPointsByJolpicaId);

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
