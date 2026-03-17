using F1Fantasy.Api.DTOs.Auth;

namespace F1Fantasy.Api.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<UserDto?> GetCurrentUserAsync(int UserId);
    }
}
