using HabitTracker.Api.Entities;

namespace HabitTracker.Api.DTOs.Tags
{
    public static class TagMappings
    {
        public static TagDto ToDto(this Tag tag)
        {
            TagDto tagDto = new()
            {
                Id = tag.Id,
                Name = tag.Name,
                Description = tag.Description,
                CreatedAtUtc = tag.CreatedAtUtc,
                UpdatedAtUtc = tag.UpdatedAtUtc
            };

            return tagDto;
        }

        public static Tag ToEntity(this CreateTagDto createTagDto, string userId)
        {
            Tag tag = new()
            {
                Id = $"t_{Guid.CreateVersion7()}",
                UserId = userId,
                Name = createTagDto.Name,
                Description = createTagDto.Description,
                CreatedAtUtc = DateTime.UtcNow
            };

            return tag;
        }

        public static void UpdateFromDto(this Tag tag, UpdateTagDto updateTagDto)
        {
            tag.Name = updateTagDto.Name;
            tag.Description = updateTagDto.Description;
            tag.UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
