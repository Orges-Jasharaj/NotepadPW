using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DemoProject.Client.Exctension
{
    public static class ClaimsExtensions
    {
        public static string? GetUserFullName(this ClaimsPrincipal? user)
        {
            if (user is null || user.Identity is null || user.Identity.IsAuthenticated == false)
                return null;

            // Common single "name" claims
            var name = user.FindFirst(JwtRegisteredClaimNames.Name)?.Value;


            if (!string.IsNullOrWhiteSpace(name))
                return name;

            // Fallbacks
            return user.FindFirst(JwtRegisteredClaimNames.Email)?.Value ?? "";
        }

        public static string? GetUserEmail(this ClaimsPrincipal? user)
        {
            if (user is null  || user.Identity is null || user.Identity.IsAuthenticated == false)
            {
                return null;
            }

            // Common single "email" claims
            var email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value;


            if (!string.IsNullOrWhiteSpace(email))
                return email;

            return "";
        }
    }
}
