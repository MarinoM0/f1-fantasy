using F1Fantasy.Api.Data;
using F1Fantasy.Api.DTOs.Predictions;
using F1Fantasy.Api.Interfaces;
using F1Fantasy.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace F1Fantasy.Api.Services
{
    public class PredictionService : IPredictionService
    {

        private const decimal PointsExactPosition = 5m;            
        private const decimal PointsWrongPositionButOnPodium = 2m;
        private const decimal PointsPerfectPodiumBonus = 5m;     
        private const int LeaderboardPageSize = 50;

        private readonly AppDbContext _dbContext;
        private readonly ILogger<PredictionService> _logger;

        public PredictionService(AppDbContext dbContext, ILogger<PredictionService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // ============ Queries (read) =================================

        public async Task<UpcomingPredictionDto> GetUpcomingAsync(
            int userId, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            var race = await _dbContext.Races
                .AsNoTracking()
                .Where(r => !r.IsCompleted && r.StartTimeUtc > now)
                .OrderBy(r => r.StartTimeUtc)
                .FirstOrDefaultAsync(ct);

            var availableDrivers = await GetAvailableDriversAsync(ct);

            if (race is null)
            {
                return new UpcomingPredictionDto
                {
                    Race = null,
                    ExistingPrediction = null,
                    AvailableDrivers = availableDrivers,
                };
            }

            var existing = await _dbContext.Predictions
                .AsNoTracking()
                .Include(p => p.Race)
                .Include(p => p.P1Driver)
                .Include(p => p.P2Driver)
                .Include(p => p.P3Driver)
                .FirstOrDefaultAsync(p => p.UserId == userId && p.RaceId == race.Id, ct);

            return new UpcomingPredictionDto
            {
                Race = BuildRaceDto(race),
                ExistingPrediction = existing is null ? null : BuildPredictionDto(existing),
                AvailableDrivers = availableDrivers,
            };
        }

        public async Task<IReadOnlyList<PredictionDto>> GetMyPredictionsAsync(
            int userId, CancellationToken ct = default)
        {
            // Score user's pending predictions before reading them back.
            await ScorePendingPredictionsAsync(userId, ct);

            var predictions = await _dbContext.Predictions
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Include(p => p.Race)
                    .ThenInclude(r => r.RaceResult!)
                    .ThenInclude(rr => rr.DriverResults)
                    .ThenInclude(dr => dr.Driver)
                .Include(p => p.P1Driver)
                .Include(p => p.P2Driver)
                .Include(p => p.P3Driver)
                .OrderByDescending(p => p.Race.RoundNumber)
                .ToListAsync(ct);

            return predictions.Select(BuildPredictionDto).ToList();
        }

        public async Task<IReadOnlyList<PredictionLeaderboardEntryDto>> GetLeaderboardAsync(
            CancellationToken ct = default)
        {
            await ScorePendingPredictionsAsync(userIdFilter: null, ct);

            var grouped = await _dbContext.Predictions
                .AsNoTracking()
                .Where(p => p.IsScored)
                .GroupBy(p => p.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalPoints = g.Sum(p => p.Score ?? 0m),
                    PredictionsScored = g.Count(),
                })
                .ToListAsync(ct);

            if (grouped.Count == 0)
            {
                return new List<PredictionLeaderboardEntryDto>();
            }

            var userIds = grouped.Select(x => x.UserId).ToList();
            var usernames = await _dbContext.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username, ct);

            var ranked = grouped
                .Select(x => new PredictionLeaderboardEntryDto
                {
                    UserId = x.UserId,
                    Username = usernames.GetValueOrDefault(x.UserId, "Unknown"),
                    TotalPoints = x.TotalPoints,
                    PredictionsScored = x.PredictionsScored,
                })
                .OrderByDescending(e => e.TotalPoints)
                .ThenBy(e => e.Username)
                .Take(LeaderboardPageSize)
                .ToList();

            for (var i = 0; i < ranked.Count; i++)
            {
                ranked[i].Rank = i + 1;
            }

            return ranked;
        }

        // ======================= Command (write) ============

        public async Task<PredictionDto> SubmitAsync(
            int userId, CreatePredictionRequestDto request, CancellationToken ct = default)
        {
            var driverIds = new[] { request.P1DriverId, request.P2DriverId, request.P3DriverId };
            if (driverIds.Distinct().Count() != 3)
            {
                throw new ArgumentException("Your podium must contain three different drivers.");
            }

            var race = await _dbContext.Races
                .FirstOrDefaultAsync(r => r.Id == request.RaceId, ct);

            if (race is null)
            {
                throw new KeyNotFoundException("Race not found.");
            }

            if (race.IsCompleted || race.StartTimeUtc <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("Predictions for this race are locked.");
            }

            var existingDriverCount = await _dbContext.Drivers
                .CountAsync(d => driverIds.Contains(d.Id), ct);

            if (existingDriverCount != 3)
            {
                throw new ArgumentException("One or more selected drivers do not exist.");
            }

            var prediction = await _dbContext.Predictions
                .FirstOrDefaultAsync(p => p.UserId == userId && p.RaceId == request.RaceId, ct);

            if (prediction is null)
            {
                prediction = new Prediction
                {
                    UserId = userId,
                    RaceId = request.RaceId,
                    P1DriverId = request.P1DriverId,
                    P2DriverId = request.P2DriverId,
                    P3DriverId = request.P3DriverId,
                };
                _dbContext.Predictions.Add(prediction);
            }
            else
            {
                prediction.P1DriverId = request.P1DriverId;
                prediction.P2DriverId = request.P2DriverId;
                prediction.P3DriverId = request.P3DriverId;
                prediction.UpdatedAtUtc = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync(ct);

            return await GetPredictionDtoByIdAsync(prediction.Id, ct);
        }

        // ============ Scoring engine ============

        // Scores every unscored prediction whose race already has results.

        private async Task ScorePendingPredictionsAsync(int? userIdFilter, CancellationToken ct)
        {
            var query = _dbContext.Predictions
                .Where(p => !p.IsScored && p.Race.RaceResult != null);

            if (userIdFilter.HasValue)
            {
                query = query.Where(p => p.UserId == userIdFilter.Value);
            }

            var pending = await query
                .Include(p => p.Race)
                    .ThenInclude(r => r.RaceResult!)
                    .ThenInclude(rr => rr.DriverResults)
                .ToListAsync(ct);

            if (pending.Count == 0)
            {
                return;
            }

            var anyScored = false;

            foreach (var prediction in pending)
            {
                var actual = GetActualPodium(prediction.Race.RaceResult!);

                // If the result row exists but doesn't have a full top-3 yet,
                // skip — we'll try again on the next read.
                if (actual.P1DriverId is null || actual.P2DriverId is null || actual.P3DriverId is null)
                {
                    continue;
                }

                prediction.Score = CalculateScore(prediction, actual);
                prediction.IsScored = true;
                prediction.ScoredAtUtc = DateTime.UtcNow;
                anyScored = true;
            }

            if (anyScored)
            {
                await _dbContext.SaveChangesAsync(ct);
            }
        }

        private static decimal CalculateScore(Prediction prediction, ActualPodium actual)
        {
            decimal score = 0m;

            var p1Exact = prediction.P1DriverId == actual.P1DriverId;
            var p2Exact = prediction.P2DriverId == actual.P2DriverId;
            var p3Exact = prediction.P3DriverId == actual.P3DriverId;

            if (p1Exact) score += PointsExactPosition;
            if (p2Exact) score += PointsExactPosition;
            if (p3Exact) score += PointsExactPosition;

            var actualPodiumIds = new int?[] { actual.P1DriverId, actual.P2DriverId, actual.P3DriverId };

            score += WrongPositionBonus(prediction.P1DriverId, actual.P1DriverId, actualPodiumIds);
            score += WrongPositionBonus(prediction.P2DriverId, actual.P2DriverId, actualPodiumIds);
            score += WrongPositionBonus(prediction.P3DriverId, actual.P3DriverId, actualPodiumIds);

            if (p1Exact && p2Exact && p3Exact)
            {
                score += PointsPerfectPodiumBonus;
            }

            return score;
        }

        private static decimal WrongPositionBonus(
            int predictedDriverId, int? actualAtThisSlot, int?[] actualPodiumIds)
        {
            if (predictedDriverId == actualAtThisSlot)
            {
                return 0m;
            }

            return actualPodiumIds.Contains(predictedDriverId)
                ? PointsWrongPositionButOnPodium
                : 0m;
        }

        private static ActualPodium GetActualPodium(RaceResult raceResult)
        {
            var byPosition = raceResult.DriverResults
                .Where(d => d.Position >= 1 && d.Position <= 3)
                .ToDictionary(d => d.Position, d => d.DriverId);

            return new ActualPodium
            {
                P1DriverId = byPosition.TryGetValue(1, out var p1) ? p1 : null,
                P2DriverId = byPosition.TryGetValue(2, out var p2) ? p2 : null,
                P3DriverId = byPosition.TryGetValue(3, out var p3) ? p3 : null,
            };
        }


        private sealed class ActualPodium
        {
            public int? P1DriverId { get; init; }
            public int? P2DriverId { get; init; }
            public int? P3DriverId { get; init; }
        }

        // ============ Private mappers / helpers ================================

        private async Task<IReadOnlyList<PredictionDriverDto>> GetAvailableDriversAsync(
            CancellationToken ct)
        {
            return await _dbContext.Drivers
                .AsNoTracking()
                .OrderBy(d => d.LastName)
                .Select(d => new PredictionDriverDto
                {
                    Id = d.Id,
                    Code = d.Code,
                    Name = d.FirstName + " " + d.LastName,
                })
                .ToListAsync(ct);
        }

        private async Task<PredictionDto> GetPredictionDtoByIdAsync(int predictionId, CancellationToken ct)
        {
            var prediction = await _dbContext.Predictions
                .AsNoTracking()
                .Include(p => p.Race)
                    .ThenInclude(r => r.RaceResult!)
                    .ThenInclude(rr => rr.DriverResults)
                    .ThenInclude(dr => dr.Driver)
                .Include(p => p.P1Driver)
                .Include(p => p.P2Driver)
                .Include(p => p.P3Driver)
                .FirstOrDefaultAsync(p => p.Id == predictionId, ct);

            if (prediction is null)
            {
                throw new KeyNotFoundException("Prediction not found.");
            }

            return BuildPredictionDto(prediction);
        }

        private static PredictionDto BuildPredictionDto(Prediction p)
        {
            var dto = new PredictionDto
            {
                Id = p.Id,
                Race = BuildRaceDto(p.Race),
                PredictedP1 = BuildDriverDto(p.P1Driver),
                PredictedP2 = BuildDriverDto(p.P2Driver),
                PredictedP3 = BuildDriverDto(p.P3Driver),
                IsScored = p.IsScored,
                Score = p.Score,
            };


            if (p.IsScored && p.Race.RaceResult is not null)
            {
                var podium = p.Race.RaceResult.DriverResults
                    .Where(d => d.Position >= 1 && d.Position <= 3)
                    .OrderBy(d => d.Position)
                    .ToList();

                dto.ActualP1 = podium.Count > 0 ? BuildDriverDto(podium[0].Driver) : null;
                dto.ActualP2 = podium.Count > 1 ? BuildDriverDto(podium[1].Driver) : null;
                dto.ActualP3 = podium.Count > 2 ? BuildDriverDto(podium[2].Driver) : null;
            }

            return dto;
        }

        private static PredictionRaceDto BuildRaceDto(Race r)
        {
            return new PredictionRaceDto
            {
                Id = r.Id,
                RoundNumber = r.RoundNumber,
                Name = r.Name,
                Country = r.Country,
                StartTimeUtc = r.StartTimeUtc,
                IsLocked = r.StartTimeUtc <= DateTime.UtcNow,
                IsCompleted = r.IsCompleted,
            };
        }

        private static PredictionDriverDto BuildDriverDto(Driver d)
        {
            return new PredictionDriverDto
            {
                Id = d.Id,
                Code = d.Code,
                Name = d.FirstName + " " + d.LastName,
            };
        }
    }
}