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
    }
}
