using DemoProject.API.Data.Models;
using DemoProject.API.Services.Interface;
using DemoProject.DataModels.Dto.Response;
using DemoProject.DataModels.Dto.System;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DemoProject.API.Services.Implementation
{
    public class TokenService : ITokenService
    {

        private readonly JwtSettings _jwtSettings;

        public TokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }
        public string GenerateAccessToken(ApplicationUser user, List<string> roles)
        {
            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.UserName),
                new Claim(JwtRegisteredClaimNames.Name, user.FirstName + " " + user.LastName),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenOptions = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.ExpirationInMinutes),
                signingCredentials: signingCredentials);

            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return token;
        }
        public RefreshTokenDto GenerateRrefreshToken()
        {
            var randomNumber = new byte[64];
            using (var numberGenerator = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                numberGenerator.GetBytes(randomNumber);
            }
            var expirationDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);

            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = Convert.ToBase64String(randomNumber),
                RefreshTokenExipirityDate = expirationDate
            };
            return refreshTokenDto;
        }
        public ClaimsPrincipal GetClaimsPrincipal(string token)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,

                IssuerSigningKey = securityKey,

            };
            return new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
        }
    }
}
