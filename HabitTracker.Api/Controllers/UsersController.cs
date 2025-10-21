using HabitTracker.Api.Database;
using HabitTracker.Api.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public sealed class UsersController(AppDbContext dbContext) : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(string id)
        {
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
    }
}
