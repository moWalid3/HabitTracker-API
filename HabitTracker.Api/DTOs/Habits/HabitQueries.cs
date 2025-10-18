using HabitTracker.Api.Entities;
using System.Linq.Expressions;

namespace HabitTracker.Api.DTOs.Habits
{
    public static class HabitQueries
    {
        public static Expression<Func<Habit, HabitDto>> ProjectToDto()
        {
            return habit => habit.ToDto();
        }

        public static Expression<Func<Habit, HabitWithTagsDto>> ProjectToDtoWithTags()
        {
            return habit => new HabitWithTagsDto
            {
                Id = habit.Id,
                Name = habit.Name,
                Description = habit.Description,
                Type = habit.Type,
                Frequency = new FrequencyDto
                {
                    Type = habit.Frequency.Type,
                    TimesPerPeriod = habit.Frequency.TimesPerPeriod
                },
                Target = new TargetDto
                {
                    Value = habit.Target.Value,
                    Unit = habit.Target.Unit
                },
                Status = habit.Status,
                IsArchived = habit.IsArchived,
                EndDate = habit.EndDate,
                Milestone = habit.Milestone == null ? null : new MilestoneDto
                {
                    Target = habit.Milestone.Target,
                    Current = habit.Milestone.Current
                },
                CreatedAtUtc = habit.CreatedAtUtc,
                UpdatedAtUtc = habit.UpdatedAtUtc,
                LastCompletedAtUtc = habit.LastCompletedAtUtc,
                Tags = habit.Tags.Select(t => t.Name).ToArray()
            };
        }
    }
}
