using F1Fantasy.Api.Data;
using F1Fantasy.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace F1Fantasy.Api.Services
{
    // Turns raw race results into fantasy points, modelled on the official F1
    // Fantasy scoring: a finishing-position table, +/-1 per position gained/lost
    // vs the grid, a fastest-lap bonus, and a DNF penalty. Driver-of-the-Day and
    // pit-stop bonuses are intentionally omitted (Jolpica does not expose them).
    public class FantasyScoringService : IFantasyScoringService
    {
        private const decimal PointsPerPositionGained = 1m;
        private const decimal FastestLapBonus = 10m;
        private const decimal DnfPenalty = -20m;

        private readonly AppDbContext _dbContext;

        public FantasyScoringService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Pure function — no I/O, so it is trivial to unit-test in isolation.
        public static decimal CalculateDriverRacePoints(int position, int grid, bool didFinish, bool fastestLap)
        {
            if (!didFinish)
            {
                return DnfPenalty;
            }

            var points = FinishPositionPoints(position);

            // Positions gained (grid ahead of finish) add points; positions lost
            // subtract them. Skipped for pit-lane starts (grid 0) where there is
            // no meaningful grid slot.
            if (grid >= 1 && position >= 1)
            {
                points += (grid - position) * PointsPerPositionGained;
            }

            if (fastestLap)
            {
                points += FastestLapBonus;
            }

            return points;
        }

        public async Task<int> ScoreAllResultsAsync(CancellationToken ct = default)
        {
            var rows = await _dbContext.RaceResultDrivers.ToListAsync(ct);

            foreach (var row in rows)
            {
                row.FantasyPoints = CalculateDriverRacePoints(
                    row.Position, row.Grid, row.DidFinish, row.FastestLap);
            }

            if (rows.Count > 0)
            {
                await _dbContext.SaveChangesAsync(ct);
            }

            return rows.Count;
        }

        public async Task<IReadOnlyDictionary<string, decimal>> BuildDriverPointsByCodeAsync(
            CancellationToken ct = default)
        {
            var totalsByDriverId = await _dbContext.RaceResultDrivers
                .AsNoTracking()
                .GroupBy(r => r.DriverId)
                .Select(g => new { DriverId = g.Key, Total = g.Sum(x => x.FantasyPoints) })
                .ToListAsync(ct);

            if (totalsByDriverId.Count == 0)
            {
                return new Dictionary<string, decimal>();
            }

            var codeById = await _dbContext.Drivers
                .AsNoTracking()
                .Select(d => new { d.Id, d.Code })
                .ToDictionaryAsync(x => x.Id, x => x.Code, ct);

            var result = new Dictionary<string, decimal>();

            foreach (var entry in totalsByDriverId)
            {
                if (codeById.TryGetValue(entry.DriverId, out var code) && !string.IsNullOrWhiteSpace(code))
                {
                    result[NormalizeKey(code)] = entry.Total;
                }
            }

            return result;
        }

        public async Task<IReadOnlyDictionary<string, decimal>> BuildConstructorPointsByJolpicaIdAsync(
            CancellationToken ct = default)
        {
            var totalsByConstructorId = await _dbContext.RaceResultDrivers
                .AsNoTracking()
                .GroupBy(r => r.ConstructorId)
                .Select(g => new { ConstructorId = g.Key, Total = g.Sum(x => x.FantasyPoints) })
                .ToListAsync(ct);

            if (totalsByConstructorId.Count == 0)
            {
                return new Dictionary<string, decimal>();
            }

            var jolpicaIdById = await _dbContext.Constructors
                .AsNoTracking()
                .Select(c => new { c.Id, c.JolpicaConstructorId })
                .ToDictionaryAsync(x => x.Id, x => x.JolpicaConstructorId, ct);

            var result = new Dictionary<string, decimal>();

            foreach (var entry in totalsByConstructorId)
            {
                if (jolpicaIdById.TryGetValue(entry.ConstructorId, out var jolpicaId)
                    && !string.IsNullOrWhiteSpace(jolpicaId))
                {
                    result[NormalizeKey(jolpicaId)] = entry.Total;
                }
            }

            return result;
        }

        private static decimal FinishPositionPoints(int position) => position switch
        {
            1 => 25m,
            2 => 18m,
            3 => 15m,
            4 => 12m,
            5 => 10m,
            6 => 8m,
            7 => 6m,
            8 => 4m,
            9 => 2m,
            10 => 1m,
            _ => 0m,
        };

        // Must match LivePointsCalculatorService's key normalization so the
        // dictionaries line up with CalculateTeamPoints' lookups.
        private static string NormalizeKey(string value) => value.Trim().ToUpperInvariant();
    }
}
