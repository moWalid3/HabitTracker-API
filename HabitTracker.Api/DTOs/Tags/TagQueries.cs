using HabitTracker.Api.Entities;
using System.Linq.Expressions;

namespace HabitTracker.Api.DTOs.Tags
{
    public static class TagQueries
    {
        public static Expression<Func<Tag, TagDto>> ProjectToDto()
        {
            return tag => tag.ToDto();
        }
    }
}
