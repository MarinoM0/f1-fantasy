namespace F1Fantasy.Api.Interfaces
{
    public interface IPricingService
    {
        // Recomputes every driver and constructor price from accumulated form
        // and records a price-history snapshot. Returns how many prices were set.
        Task<int> RecalculatePricesAsync(CancellationToken ct = default);
    }
}
