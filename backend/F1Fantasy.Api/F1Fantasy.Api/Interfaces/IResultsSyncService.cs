namespace F1Fantasy.Api.Interfaces
{
    public interface IResultsSyncService
    {
        Task<int> SyncCompletedRacesAsync(CancellationToken ct = default);
    }
}
