using F1Fantasy.Api.DTOs.Dashboard;
using F1Fantasy.Api.DTOs.Jolpica;


namespace F1Fantasy.Api.Interfaces
{
    public interface IJolpicaService
    {
        Task<IReadOnlyList<JolpicaDriverStandingDto>> GetDriverStandingsAsync(
        string season = "current",
        CancellationToken cancellationToken = default);

        Task<IReadOnlyList<JolpicaConstructorStandingDto>> GetConstructorStandingsAsync(
            string season = "current",
            CancellationToken cancellationToken = default);

        Task<JolpicaRaceResultDto?> GetRaceResultsAsync(
            string season = "current",
            string round = "last",
            CancellationToken cancellationToken = default);

        Task<DashboardUpcomingRaceDto?> GetNextRaceAsync(
            string season = "current",
            CancellationToken cancellationToken = default);
    }
}
