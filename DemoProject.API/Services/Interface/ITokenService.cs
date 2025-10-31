using DemoProject.API.Data.Models;
using DemoProject.DataModels.Dto.Response;
using System.Security.Claims;

namespace DemoProject.API.Services.Interface
{
    public interface ITokenService
    {
        string GenerateAccessToken(ApplicationUser user, List<string> roles);
        RefreshTokenDto GenerateRrefreshToken();
        ClaimsPrincipal GetClaimsPrincipal(string token);
    }
}
