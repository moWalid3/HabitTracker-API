using HabitTracker.Api.Database;
using HabitTracker.Api.DTOs.Habits;
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
                .Select(h => new HabitDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Description = h.Description,
                    Type = h.Type,
                    Frequency = new FrequencyDto
                    {
                        Type = h.Frequency.Type,
                        TimesPerPeriod = h.Frequency.TimesPerPeriod
                    },
                    Target = new TargetDto
                    {
                        Value = h.Target.Value,
                        Unit = h.Target.Unit
                    },
                    Status = h.Status,
                    IsArchived = h.IsArchived,
                    EndDate = h.EndDate,
                    Milestone = h.Milestone == null ? null : new MilestoneDto
                    {
                        Target = h.Milestone.Target,
                        Current = h.Milestone.Current
                    },
                    CreatedAtUtc = h.CreatedAtUtc,
                    UpdatedAtUtc = h.UpdatedAtUtc,
                    LastCompletedAtUtc = h.LastCompletedAtUtc
                })
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
                .Select(h => new HabitDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Description = h.Description,
                    Type = h.Type,
                    Frequency = new FrequencyDto
                    {
                        Type = h.Frequency.Type,
                        TimesPerPeriod = h.Frequency.TimesPerPeriod
                    },
                    Target = new TargetDto
                    {
                        Value = h.Target.Value,
                        Unit = h.Target.Unit
                    },
                    Status = h.Status,
                    IsArchived = h.IsArchived,
                    EndDate = h.EndDate,
                    Milestone = h.Milestone == null ? null : new MilestoneDto
                    {
                        Target = h.Milestone.Target,
                        Current = h.Milestone.Current
                    },
                    CreatedAtUtc = h.CreatedAtUtc,
                    UpdatedAtUtc = h.UpdatedAtUtc,
                    LastCompletedAtUtc = h.LastCompletedAtUtc
                })
                .FirstOrDefaultAsync();

            if (habit == null)
            {
                return NotFound();
            }

            return Ok(habit);
        }
    }
}
