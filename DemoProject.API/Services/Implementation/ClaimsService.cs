using DemoProject.API.Services.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DemoProject.API.Services.Implementation
{
    public class ClaimsService : IClaimsService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimsService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetCurrentUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var userPrincipal = httpContext.User;

            var userId = userPrincipal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? string.Empty;

            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            return userId;
        }
    }
}
