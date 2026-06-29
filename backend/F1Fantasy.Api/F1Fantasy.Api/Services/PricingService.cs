using F1Fantasy.Api.Data;
using F1Fantasy.Api.Interfaces;
using F1Fantasy.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace F1Fantasy.Api.Services
{
    // Performance-based dynamic pricing. Real F1 Fantasy moves prices on transfer
    // demand, which needs a large player base; with a single instance we instead
    // drift each price away from its seeded BasePrice according to how the driver
    // (or constructor) is scoring relative to the field. Deterministic: the price
    // is a pure function of current results, so re-running is safe.
    public class PricingService : IPricingService
    {
        // Price change per fantasy-point-per-race above (or below) the field average.
        private const decimal Sensitivity = 0.15m;

        // Price is clamped to this band around the seeded base price.
        private const decimal MinPriceFactor = 0.5m;
        private const decimal MaxPriceFactor = 1.6m;

        private readonly AppDbContext _dbContext;

        public PricingService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> RecalculatePricesAsync(CancellationToken ct = default)
        {
            var completedRaceCount = await _dbContext.Races.CountAsync(r => r.IsCompleted, ct);
            if (completedRaceCount == 0)
            {
                // No results yet — nothing to price on, leave seeded prices as-is.
                return 0;
            }

            var pointsByDriverId = await SumFantasyPointsAsync(r => r.DriverId, ct);
            var pointsByConstructorId = await SumFantasyPointsAsync(r => r.ConstructorId, ct);

            var drivers = await _dbContext.Drivers.ToListAsync(ct);
            var constructors = await _dbContext.Constructors.ToListAsync(ct);

            ApplyPricing(
                drivers,
                driver => AveragePerRace(pointsByDriverId, driver.Id, completedRaceCount),
                driver => AnchorPrice(driver.BasePrice, driver.Price),
                (driver, price) => driver.Price = price);

            ApplyPricing(
                constructors,
                constructor => AveragePerRace(pointsByConstructorId, constructor.Id, completedRaceCount),
                constructor => AnchorPrice(constructor.BasePrice, constructor.Price),
                (constructor, price) => constructor.Price = price);

            foreach (var driver in drivers)
            {
                _dbContext.PriceHistories.Add(new PriceHistory { DriverId = driver.Id, Price = driver.Price });
            }

            foreach (var constructor in constructors)
            {
                _dbContext.PriceHistories.Add(new PriceHistory { ConstructorId = constructor.Id, Price = constructor.Price });
            }

            await _dbContext.SaveChangesAsync(ct);

            return drivers.Count + constructors.Count;
        }

        // Sums FantasyPoints grouped by the given key selector (driver or constructor).
        private async Task<IReadOnlyDictionary<int, decimal>> SumFantasyPointsAsync(
            System.Linq.Expressions.Expression<Func<RaceResultDriver, int>> keySelector,
            CancellationToken ct)
        {
            return await _dbContext.RaceResultDrivers
                .AsNoTracking()
                .GroupBy(keySelector)
                .Select(g => new { Key = g.Key, Total = g.Sum(x => x.FantasyPoints) })
                .ToDictionaryAsync(x => x.Key, x => x.Total, ct);
        }

        private static void ApplyPricing<T>(
            IReadOnlyList<T> items,
            Func<T, decimal> averageSelector,
            Func<T, decimal> anchorSelector,
            Action<T, decimal> setPrice)
        {
            var fieldAverage = items.Select(averageSelector).DefaultIfEmpty(0m).Average();

            foreach (var item in items)
            {
                var anchor = anchorSelector(item);
                var performance = averageSelector(item) - fieldAverage;

                var raw = anchor + performance * Sensitivity;
                var clamped = Math.Clamp(raw, anchor * MinPriceFactor, anchor * MaxPriceFactor);

                setPrice(item, Math.Round(clamped, 1));
            }
        }

        private static decimal AveragePerRace(
            IReadOnlyDictionary<int, decimal> totals, int id, int raceCount)
        {
            return totals.TryGetValue(id, out var total) ? total / raceCount : 0m;
        }

        // Old rows seeded before BasePrice existed fall back to their current price.
        private static decimal AnchorPrice(decimal basePrice, decimal currentPrice)
        {
            return basePrice > 0 ? basePrice : currentPrice;
        }
    }
}
