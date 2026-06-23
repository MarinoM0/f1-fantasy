namespace F1Fantasy.Api.Interfaces
{
    public interface IFantasyScoringService
    {
        // Recomputes fantasy points for every stored race result. Idempotent —
        // the score is a pure function of the result row, so re-running is safe.
        Task<int> ScoreAllResultsAsync(CancellationToken ct = default);

        // Cumulative fantasy points per driver, keyed by normalized driver code.
        Task<IReadOnlyDictionary<string, decimal>> BuildDriverPointsByCodeAsync(CancellationToken ct = default);

        // Cumulative fantasy points per constructor (sum of its drivers),
        // keyed by normalized Jolpica constructor id.
        Task<IReadOnlyDictionary<string, decimal>> BuildConstructorPointsByJolpicaIdAsync(CancellationToken ct = default);
    }
}
