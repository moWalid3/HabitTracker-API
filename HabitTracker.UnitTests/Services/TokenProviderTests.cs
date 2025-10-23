using HabitTracker.Api.DTOs.Auth;
using HabitTracker.Api.Entities;
using HabitTracker.Api.Services;
using HabitTracker.Api.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HabitTracker.UnitTests.Services
{
    public sealed class TokenProviderTests
    {
        private readonly TokenProvider tokenProvider;
        private readonly JwtAuthOptions jwtAuthOptions;

        public TokenProviderTests()
        {
            IOptions<JwtAuthOptions> options = Options.Create(new JwtAuthOptions()
            {
                Key = "your-secret-key-here-that-should-also-be-fairly-long",
                Issuer = "test-issuer",
                Audience = "test-audience",
                ExpirationInMinutes = 30,
                RefreshTokenExpirationDays = 7,
            });

            jwtAuthOptions = options.Value;
            tokenProvider = new(options);
        }

        [Fact]
        public void Create_ShouldReturnAccessTokens()
        {
            TokenRequest tokenRequest = new(User.CreateNewId(), "test@example.com", [Roles.Member]);

            // Act
            AccessTokensDto accessTokensDto = tokenProvider.Create(tokenRequest);

            // Assert
            Assert.NotNull(accessTokensDto.AccessToken);
            Assert.NotNull(accessTokensDto.RefreshToken);
        }

        [Fact]
        public void Create_ShouldGenerateValidAccessToken()
        {
            // Arrange
            TokenRequest tokenRequest = new(User.CreateNewId(), "test@example.com", [Roles.Member]);

            // Act
            AccessTokensDto accessTokensDto = tokenProvider.Create(tokenRequest);

            // Assert
            JwtSecurityTokenHandler handler = new();

            TokenValidationParameters validationParameters = new()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthOptions.Key)),
                ValidateIssuer = true,
                ValidIssuer = jwtAuthOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtAuthOptions.Audience,
                NameClaimType = JwtRegisteredClaimNames.Email,
            };

            ClaimsPrincipal claimsPrincipal = handler.ValidateToken(
                accessTokensDto.AccessToken,
                validationParameters,
                out SecurityToken validatedToken);

            Assert.NotNull(validatedToken);
            Assert.Equal(tokenRequest.UserId, claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));
            Assert.Equal(tokenRequest.Email, claimsPrincipal.FindFirstValue(ClaimTypes.Email));
            Assert.Contains(claimsPrincipal.FindAll(ClaimTypes.Role), claim => claim.Value == Roles.Member);
        }

        [Fact]
        public void Create_ShouldGenerateUniqueRefreshTokens()
        {
            // Arrange
            TokenRequest tokenRequest = new(User.CreateNewId(), "test@example.com", [Roles.Member]);

            // Act
            AccessTokensDto accessTokensDto1 = tokenProvider.Create(tokenRequest);
            AccessTokensDto accessTokensDto2 = tokenProvider.Create(tokenRequest);

            // Assert
            Assert.NotEqual(accessTokensDto1.RefreshToken, accessTokensDto2.RefreshToken);
        }

        [Fact]
        public void Create_ShouldGenerateAccessTokenWithCorrectExpiration()
        {
            // Arrange
            TokenRequest tokenRequest = new(User.CreateNewId(), "test@example.com", [Roles.Member]);

            // Act
            AccessTokensDto accessTokensDto = tokenProvider.Create(tokenRequest);

            // Assert
            JwtSecurityTokenHandler handler = new();
            JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(accessTokensDto.AccessToken);

            DateTime expectedExpiration = DateTime.UtcNow.AddMinutes(jwtAuthOptions.ExpirationInMinutes);
            DateTime actualExpiration = jwtSecurityToken.ValidTo;

            // Allow for a small time difference due to test execution
            Assert.True(Math.Abs((expectedExpiration - actualExpiration).TotalSeconds) < 3);
        }

        [Fact]
        public void Create_ShouldGenerateBase64RefreshToken()
        {
            // Arrange
            TokenRequest tokenRequest = new(User.CreateNewId(), "test@example.com", [Roles.Member]);

            // Act
            AccessTokensDto accessTokensDto = tokenProvider.Create(tokenRequest);

            // Assert
            Assert.True(IsBase64String(accessTokensDto.RefreshToken));
        }

        private static bool IsBase64String(string base64)
        {
            Span<byte> buffer = new byte[base64.Length];
            return Convert.TryFromBase64String(base64, buffer, out _);
        }
    }

}
