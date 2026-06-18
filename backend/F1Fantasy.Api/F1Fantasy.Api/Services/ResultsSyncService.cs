using F1Fantasy.Api.Data;
using F1Fantasy.Api.DTOs.Jolpica;
using F1Fantasy.Api.Interfaces;
using F1Fantasy.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace F1Fantasy.Api.Services
{
    // Pulls finished-race results from Jolpica into RaceResult / RaceResultDriver.
    // Only races whose start time has passed AND that Jolpica actually has results
    // for get synced. Idempotent: a race that already has a stored result is skipped.
    public class ResultsSyncService : IResultsSyncService
    {
        private const string CurrentSeason = "current";

        private readonly AppDbContext _dbContext;
        private readonly IJolpicaService _jolpicaService;
        private readonly ILogger<ResultsSyncService> _logger;

        public ResultsSyncService(
            AppDbContext dbContext,
            IJolpicaService jolpicaService,
            ILogger<ResultsSyncService> logger)
        {
            _dbContext = dbContext;
            _jolpicaService = jolpicaService;
            _logger = logger;
        }

        public async Task<int> SyncCompletedRacesAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            var pendingRaces = await _dbContext.Races
                .Include(r => r.RaceResult)
                .Where(r => r.StartTimeUtc <= now && r.RaceResult == null)
                .OrderBy(r => r.RoundNumber)
                .ToListAsync(ct);

            if (pendingRaces.Count == 0)
            {
                return 0;
            }

            // Map our drivers by their code so we can match Jolpica result rows.
            var driversByCode = await _dbContext.Drivers
                .AsNoTracking()
                .ToDictionaryAsync(d => NormalizeCode(d.Code), d => d, ct);

            var syncedCount = 0;

            foreach (var race in pendingRaces)
            {
                var jolpicaResult = await FetchResultsSafelyAsync(race.RoundNumber, ct);
                if (jolpicaResult is null || jolpicaResult.Results.Count == 0)
                {
                    // Race hasn't actually run yet (or Jolpica has no data) — leave it
                    // pending so a later sync can pick it up.
                    continue;
                }

                var driverResults = BuildDriverResults(jolpicaResult, driversByCode);
                if (driverResults.Count == 0)
                {
                    continue;
                }

                _dbContext.RaceResults.Add(new RaceResult
                {
                    RaceId = race.Id,
                    DriverResults = driverResults,
                });

                race.IsCompleted = true;
                syncedCount++;
            }

            if (syncedCount > 0)
            {
                await _dbContext.SaveChangesAsync(ct);
            }

            return syncedCount;
        }

        private async Task<JolpicaRaceResultDto?> FetchResultsSafelyAsync(int round, CancellationToken ct)
        {
            try
            {
                return await _jolpicaService.GetRaceResultsAsync(CurrentSeason, round.ToString(), ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch Jolpica results for round {Round}", round);
                return null;
            }
        }

        private static List<RaceResultDriver> BuildDriverResults(
            JolpicaRaceResultDto jolpicaResult,
            IReadOnlyDictionary<string, Driver> driversByCode)
        {
            var rows = new List<RaceResultDriver>();
            var seenDriverIds = new HashSet<int>();

            foreach (var entry in jolpicaResult.Results)
            {
                if (string.IsNullOrWhiteSpace(entry.DriverCode))
                {
                    continue;
                }

                if (!driversByCode.TryGetValue(NormalizeCode(entry.DriverCode), out var driver))
                {
                    // A driver Jolpica lists but we haven't seeded — skip them.
                    continue;
                }

                // Composite key is (RaceResultId, DriverId), so guard against duplicates.
                if (!seenDriverIds.Add(driver.Id))
                {
                    continue;
                }

                rows.Add(new RaceResultDriver
                {
                    DriverId = driver.Id,
                    ConstructorId = driver.ConstructorId,
                    Position = entry.Position ?? 0,
                    Grid = entry.Grid ?? 0,
                    DidFinish = IsFinished(entry.Status),
                    FastestLap = entry.FastestLap,
                });
            }

            return rows;
        }

        private static bool IsFinished(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return false;
            }

            // Jolpica marks classified finishers as "Finished" or "+N Lap(s)".
            // Anything else (Accident, Collision, Engine, Retired, Disqualified, ...)
            // counts as not finishing.
            return status.Equals("Finished", StringComparison.OrdinalIgnoreCase)
                || status.StartsWith("+", StringComparison.Ordinal);
        }

        private static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();
    }
}
