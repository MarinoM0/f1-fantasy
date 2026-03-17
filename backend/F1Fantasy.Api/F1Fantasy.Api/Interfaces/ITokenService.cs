using F1Fantasy.Api.Models;

namespace F1Fantasy.Api.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
        DateTime GetTokenExpiryUtc();
    }
}
