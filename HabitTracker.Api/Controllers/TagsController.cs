using FluentValidation;
using HabitTracker.Api.Database;
using HabitTracker.Api.DTOs.Tags;
using HabitTracker.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public sealed class TagsController(AppDbContext dbContext) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<TagsCollectionDto>> Get()
        {
            List<TagDto> tags = await dbContext
                .Tags
                .Select(TagQueries.ProjectToDto())
                .ToListAsync();

            TagsCollectionDto tagsCollectionDto = new() { Items = tags };

            return Ok(tagsCollectionDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TagDto>> GetById(string id)
        {
            TagDto? tag = await dbContext
                .Tags
                .Where(t => t.Id == id)
                .Select(TagQueries.ProjectToDto())
                .FirstOrDefaultAsync();

            if (tag == null)
            {
                return NotFound();
            }

            return Ok(tag);
        }

        [HttpPost]
        public async Task<ActionResult<TagDto>> Create(
            CreateTagDto createTagDto,
            IValidator<CreateTagDto> validator)
        {
            await validator.ValidateAndThrowAsync(createTagDto);

            Tag tag = createTagDto.ToEntity();

            bool isTagExisting = await dbContext.Tags.AnyAsync(t => t.Name == tag.Name);

            if (isTagExisting)
            {
                return Problem(
                    detail: $"The tag '{tag.Name}' already exists",
                    statusCode: StatusCodes.Status409Conflict);
            }

            await dbContext.Tags.AddAsync(tag);
            await dbContext.SaveChangesAsync();

            TagDto tagDto = tag.ToDto();

            return CreatedAtAction(nameof(GetById), new { id = tagDto.Id }, tagDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, UpdateTagDto updateTagDto)
        {

            Tag? tag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null)
            {
                return NotFound();
            }

            bool isTagExisting = await dbContext.Tags.AnyAsync(t => t.Name == updateTagDto.Name && t.Id != tag.Id);

            if (isTagExisting)
            {
                return Conflict($"The tag '{updateTagDto.Name}' already exists");
            }

            tag.UpdateFromDto(updateTagDto);

            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            Tag? tag = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null)
            {
                return NotFound();
            }

            dbContext.Remove(tag);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
