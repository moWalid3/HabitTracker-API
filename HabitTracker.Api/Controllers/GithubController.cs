using HabitTracker.Api.DTOs.Github;
using HabitTracker.Api.Entities;
using HabitTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Api.Controllers
{
    [Authorize(Roles = Roles.Member)]
    [Route("[controller]")]
    [ApiController]
    public class GithubController(
        GithubAccessTokenService githubAccessTokenService,
        GithubService githubService,
        UserContext userContext) : ControllerBase
    {
        [HttpPut("personal-access-token")]
        public async Task<IActionResult> StoreAccessToken(StoreGithubAccessTokenDto storeGithubAccessTokenDto)
        {
            string? userId = await userContext.GetUserIdAsync();

            if (userId == null)
            {
                return Unauthorized();
            }

            await githubAccessTokenService.StoreAsync(userId, storeGithubAccessTokenDto);

            return NoContent();
        }

        [HttpDelete("personal-access-token")]
        public async Task<IActionResult> RevokeAccessToken()
        {
            string? userId = await userContext.GetUserIdAsync();

            if (userId == null)
            {
                return Unauthorized();
            }

            await githubAccessTokenService.RevokeAsync(userId);

            return NoContent();
        }

        [HttpGet("profile")]
        public async Task<ActionResult<GithubUserProfileDto>> GetUserProfile()
        {
            string? userId = await userContext.GetUserIdAsync();

            if (userId == null)
            {
                return Unauthorized();
            }

            string? accessToken = await githubAccessTokenService.GetAsync(userId);

            if (accessToken == null)
            {
                return NotFound();
            }

            GithubUserProfileDto? userProfileDto = await githubService.GetUserProfileAsync(accessToken);

            if (userProfileDto == null)
            {
                return NotFound();
            }

            return Ok(userProfileDto);
        }
    }
}
