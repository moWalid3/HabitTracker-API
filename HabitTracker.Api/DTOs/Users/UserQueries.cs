using HabitTracker.Api.Entities;
using System.Linq.Expressions;

namespace HabitTracker.Api.DTOs.Users
{
    internal static class UserQueries
    {
        public static Expression<Func<User, UserDto>> ProjectToDto()
        {
            return user => new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAtUtc = user.CreatedAtUtc,
                UpdatedAtUtc = user.UpdatedAtUtc
            };
        }

    }
}
