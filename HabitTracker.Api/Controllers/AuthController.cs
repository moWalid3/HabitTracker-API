using HabitTracker.Api.Database;
using HabitTracker.Api.DTOs.Auth;
using HabitTracker.Api.DTOs.Users;
using HabitTracker.Api.Entities;
using HabitTracker.Api.Services;
using HabitTracker.Api.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace HabitTracker.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [AllowAnonymous]
    public sealed class AuthController(
        UserManager<IdentityUser> userManager,
        AppIdentityDbContext identityDbContext,
        AppDbContext appDbContext,
        TokenProvider tokenProvider,
        IOptions<JwtAuthOptions> options) : ControllerBase
    {
        private readonly JwtAuthOptions jwtAuthOptions = options.Value;

        [HttpPost("register")]
        public async Task<ActionResult<AccessTokensDto>> Register(RegisterUserDto registerUserDto)
        {
            using IDbContextTransaction transaction = await identityDbContext.Database.BeginTransactionAsync();
            appDbContext.Database.SetDbConnection(identityDbContext.Database.GetDbConnection());
            await appDbContext.Database.UseTransactionAsync(transaction.GetDbTransaction());

            IdentityUser identityUser = new()
            {
                UserName = registerUserDto.Email,
                Email = registerUserDto.Email,
            };

            IdentityResult identityResult = await userManager.CreateAsync(identityUser, registerUserDto.Password);

            if (!identityResult.Succeeded)
            {
                Dictionary<string, object?> extensions = new()
                {
                    {
                        "errors",
                        identityResult.Errors.ToDictionary(e => e.Code, e => e.Description)
                    }
                };

                return Problem(
                    detail: "Unable to register user, please try again",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: extensions);
            }

            User user = registerUserDto.ToEntity();

            user.IdentityId = identityUser.Id;

            await appDbContext.Users.AddAsync(user);
            await appDbContext.SaveChangesAsync();

            TokenRequest tokenRequest = new(identityUser.Id, identityUser.Email);
            AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

            RefreshToken refreshToken = new()
            {
                Id = Guid.CreateVersion7(),
                UserId = identityUser.Id,
                Token = accessTokens.RefreshToken,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(jwtAuthOptions.RefreshTokenExpirationDays)
            };

            await identityDbContext.RefreshTokens.AddAsync(refreshToken);
            await identityDbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok(accessTokens);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AccessTokensDto>> Login(LoginUserDto loginUserDto)
        {
            IdentityUser? identityUser = await userManager.FindByEmailAsync(loginUserDto.Email);

            if (identityUser == null || !await userManager.CheckPasswordAsync(identityUser, loginUserDto.Password))
            {
                return Unauthorized();
            }

            TokenRequest tokenRequest = new(identityUser.Id, identityUser.Email!);
            AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

            RefreshToken refreshToken = new()
            {
                Id = Guid.CreateVersion7(),
                UserId = identityUser.Id,
                Token = accessTokens.RefreshToken,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(jwtAuthOptions.RefreshTokenExpirationDays)
            };

            await identityDbContext.RefreshTokens.AddAsync(refreshToken);
            await identityDbContext.SaveChangesAsync();

            return Ok(accessTokens);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AccessTokensDto>> Refresh(RefreshTokenDto refreshTokenDto)
        {
            RefreshToken? refreshToken = await identityDbContext.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == refreshTokenDto.RefreshToken);

            if (refreshToken == null)
            {
                return Unauthorized();
            }

            if (refreshToken.ExpiresAtUtc < DateTime.UtcNow)
            {
                return Unauthorized();
            }

            TokenRequest tokenRequest = new(refreshToken.User.Id, refreshToken.User.Email!);
            AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

            refreshToken.Token = accessTokens.RefreshToken;
            refreshToken.ExpiresAtUtc = DateTime.UtcNow.AddDays(jwtAuthOptions.RefreshTokenExpirationDays);

            await identityDbContext.SaveChangesAsync();

            return Ok(accessTokens);
        }
    }
}
