using F1Fantasy.Api.DTOs.Leagues;

namespace F1Fantasy.Api.Interfaces
{
    public interface ILeagueService
    {
        Task<LeagueDto> CreateAsync(int userId, CreateLeagueRequestDto request, CancellationToken ct = default);
        Task<LeagueDto> JoinAsync(int userId, JoinLeagueRequestDto request, CancellationToken ct = default);
        Task LeaveAsync(int userId, int leagueId, CancellationToken ct = default);
        Task DeleteAsync(int userId, int leagueId, CancellationToken CT = default);

        Task<IReadOnlyList<LeagueSummaryDto>> GetMyLeaguesAsync(int userId, CancellationToken ct = default);
        Task<LeagueDto> GetByIdAsync(int userId, int leagueId, CancellationToken ct = default);
        Task<IReadOnlyList<LeagueLeaderboardEntryDto>> GetLeaderboardAsync(int userId, int leagueId, CancellationToken ct = default);
    }
}
