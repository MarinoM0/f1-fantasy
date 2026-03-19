using F1Fantasy.Api.DTOs;

namespace F1Fantasy.Api.Interfaces
{
    public interface IFantasyTeamService
    {
        Task<FantasyTeamDto> CreateAsync(int userId, CreateFantasyTeamRequestDto request);
        Task<FantasyTeamDto?> GetMyTeamAsync(int userId);
    }
}
