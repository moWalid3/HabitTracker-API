using HabitTracker.Api.DTOs.Auth;
using HabitTracker.Api.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HabitTracker.Api.Services
{
    public sealed class TokenProvider(IOptions<JwtAuthOptions> options)
    {
        private readonly JwtAuthOptions jwtAuthOptions = options.Value;

        public AccessTokensDto Create(TokenRequest tokenRequest)
        {
            return new AccessTokensDto(GenerateAccessToken(tokenRequest), GenerateRefreshToken());
        }

        private string GenerateAccessToken(TokenRequest tokenRequest)
        {
            List<Claim> claims = [
                new(JwtRegisteredClaimNames.Sub, tokenRequest.UserId),
                new(JwtRegisteredClaimNames.Email, tokenRequest.Email)
            ];

            SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(jwtAuthOptions.Key));

            SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256);

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(jwtAuthOptions.ExpirationInMinutes),
                SigningCredentials = signingCredentials,
                Issuer = jwtAuthOptions.Issuer,
                Audience = jwtAuthOptions.Audience
            };

            string accessToken = new JsonWebTokenHandler().CreateToken(tokenDescriptor);

            return accessToken;
        }

        private static string GenerateRefreshToken()
        {
            byte[] randomBytes = RandomNumberGenerator.GetBytes(32);

            return Convert.ToBase64String(randomBytes);
        }
    }
}
