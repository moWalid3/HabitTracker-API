using HabitTracker.Api.Database;
using HabitTracker.Api.DTOs.Habits;
using HabitTracker.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public sealed class HabitsController(AppDbContext dbContext) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<HabitsCollectionDto>> GetAll()
        {
            List<HabitDto> habits = await dbContext
                .Habits
                .Select(HabitQueries.ProjectToDto())
                .ToListAsync();

            HabitsCollectionDto habitsCollectionDto = new() { Data = habits };

            return Ok(habitsCollectionDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HabitDto>> GetById(string id)
        {
            HabitDto? habit = await dbContext
                .Habits
                .Where(h => h.Id == id)
                .Select(HabitQueries.ProjectToDto())
                .FirstOrDefaultAsync();

            if (habit == null)
            {
                return NotFound();
            }

            return Ok(habit);
        }

        [HttpPost]
        public async Task<ActionResult<HabitDto>> Create(CreateHabitDto createHabitDto)
        {
            Habit habit = createHabitDto.ToEntity();

            try
            {
                await dbContext.Habits.AddAsync(habit);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

            HabitDto habitDto = habit.ToDto();

            return CreatedAtAction(nameof(GetById), new { id = habitDto.Id }, habitDto);
        }
    }
}
