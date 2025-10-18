using HabitTracker.Api.Database;
using HabitTracker.Api.DTOs.Habits;
using HabitTracker.Api.Entities;
using Microsoft.AspNetCore.JsonPatch;
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
        public async Task<ActionResult<HabitWithTagsDto>> GetById(string id)
        {
            HabitWithTagsDto? habit = await dbContext
                .Habits
                .Where(h => h.Id == id)
                .Select(HabitQueries.ProjectToDtoWithTags())
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

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, UpdateHabitDto updateHabitDto)
        {
            Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);

            if (habit == null)
            {
                return NotFound();
            }

            habit.UpdateFromDto(updateHabitDto);

            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(string id, JsonPatchDocument<HabitDto> patchDocument)
        {
            Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);

            if (habit == null)
            {
                return NotFound();
            }

            HabitDto habitDto = habit.ToDto();

            patchDocument.ApplyTo(habitDto, ModelState);

            if (!TryValidateModel(habitDto))
            {
                return ValidationProblem(ModelState);
            }

            habit.Name = habitDto.Name;
            habit.Description = habitDto.Description;
            habit.UpdatedAtUtc = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);

            if (habit == null)
            {
                return NotFound();
            }

            dbContext.Habits.Remove(habit);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
