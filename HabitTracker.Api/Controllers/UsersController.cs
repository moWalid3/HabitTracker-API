using HabitTracker.Api.Database;
using HabitTracker.Api.DTOs.Users;
using HabitTracker.Api.Entities;
using HabitTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Controllers
{
    [Authorize(Roles = Roles.Member)]
    [Route("[controller]")]
    [ApiController]
    public sealed class UsersController(AppDbContext dbContext, UserContext userContext) : ControllerBase
    {
        [HttpGet("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<UserDto>> GetUserById(string id)
        {
            string? userId = await userContext.GetUserIdAsync();

            if (userId == null)
            {
                return Unauthorized();
            }

            if (userId != id)
            {
                return Forbid();
            }

            UserDto? user = await dbContext.Users
                .Where(u => u.Id == id)
                .Select(UserQueries.ProjectToDto())
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            string? userId = await userContext.GetUserIdAsync();

            if (userId == null)
            {
                return Unauthorized();
            }

            UserDto? user = await dbContext.Users
                .Where(u => u.Id == userId)
                .Select(UserQueries.ProjectToDto())
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }
    }
}
