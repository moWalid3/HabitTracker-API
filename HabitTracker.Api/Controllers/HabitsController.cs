using FluentValidation;
using HabitTracker.Api.Database;
using HabitTracker.Api.DTOs.Common;
using HabitTracker.Api.DTOs.Habits;
using HabitTracker.Api.Entities;
using HabitTracker.Api.Services;
using HabitTracker.Api.Services.Sorting;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Linq.Dynamic.Core;

namespace HabitTracker.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public sealed class HabitsController(AppDbContext dbContext, LinkService linkService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] HabitsQueryParameters query,
            SortMappingProvider sortMappingProvider,
            DataShapingService dataShapingService)
        {
            if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(query.Sort))
            {
                return Problem(
                    detail: $"The provided sort parameter is not valid: '{query.Sort}'",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (!dataShapingService.Validate<HabitDto>(query.Fields))
            {
                return Problem(
                    detail: $"The provided data shaping fields are not valid: '{query.Fields}'",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            SortMapping[] sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();

            query.Search ??= query.Search?.Trim().ToLower();

            IQueryable<HabitDto> habitsQuery = dbContext
                .Habits
                .Where(h => query.Search == null ||
                            h.Name.ToLower().Contains(query.Search) ||
                            h.Description != null && h.Description.ToLower().Contains(query.Search))
                .Where(h => query.Type == null || h.Type == query.Type)
                .Where(h => query.Status == null || h.Status == query.Status)
                .ApplySort(query.Sort, sortMappings)
                .Select(HabitQueries.ProjectToDto());

            int totalCount = await habitsQuery.CountAsync();

            List<HabitDto> habits = await habitsQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            PaginationResult<ExpandoObject> paginationResult = new()
            {
                Items = dataShapingService.ShapeCollectionData(
                    habits,
                    query.Fields,
                    h => CreateLinksForHabit(h.Id, query.Fields)),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };

            paginationResult.Links = CreateLinksForHabits(
                query,
                paginationResult.HasPreviousPage,
                paginationResult.HasNextPage);

            return Ok(paginationResult);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
            string id,
            string? fields,
            DataShapingService dataShapingService)
        {
            if (!dataShapingService.Validate<HabitWithTagsDto>(fields))
            {
                return Problem(
                    detail: $"The provided data shaping fields are not valid: '{fields}'",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            HabitWithTagsDto? habit = await dbContext
                .Habits
                .Where(h => h.Id == id)
                .Select(HabitQueries.ProjectToDtoWithTags())
                .FirstOrDefaultAsync();

            if (habit == null)
            {
                return NotFound();
            }

            ExpandoObject shapedHabitDto = dataShapingService.ShapeData(habit, fields);

            List<LinkDto> links = CreateLinksForHabit(id, fields);

            shapedHabitDto.TryAdd("links", links);

            return Ok(shapedHabitDto);
        }

        [HttpPost]
        public async Task<ActionResult<HabitDto>> Create(
            CreateHabitDto createHabitDto,
            IValidator<CreateHabitDto> validator)
        {
            await validator.ValidateAndThrowAsync(createHabitDto);

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

            habitDto.Links = CreateLinksForHabit(habitDto.Id, null);

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

        private List<LinkDto> CreateLinksForHabits(
            HabitsQueryParameters parameters,
            bool hasPreviousPage,
            bool hasNextPage)
        {
            List<LinkDto> links =
            [
                linkService.Create(nameof(GetAll), "self", HttpMethods.Get, new
                {
                    page = parameters.Page,
                    pageSize = parameters.PageSize,
                    fields = parameters.Fields,
                    q = parameters.Search,
                    sort = parameters.Sort,
                    type = parameters.Type,
                    status = parameters.Status
                }),
                linkService.Create(nameof(Create), "create", HttpMethods.Post)
            ];

            if (hasPreviousPage)
            {
                links.Add(linkService.Create(nameof(GetAll), "previous-page", HttpMethods.Get, new
                {
                    page = parameters.Page - 1,
                    pageSize = parameters.PageSize,
                    fields = parameters.Fields,
                    q = parameters.Search,
                    sort = parameters.Sort,
                    type = parameters.Type,
                    status = parameters.Status
                }));
            }

            if (hasNextPage)
            {
                links.Add(linkService.Create(nameof(GetAll), "next-page", HttpMethods.Get, new
                {
                    page = parameters.Page + 1,
                    pageSize = parameters.PageSize,
                    fields = parameters.Fields,
                    q = parameters.Search,
                    sort = parameters.Sort,
                    type = parameters.Type,
                    status = parameters.Status
                }));
            }

            return links;
        }

        private List<LinkDto> CreateLinksForHabit(string id, string? fields)
        {
            return [
                linkService.Create(nameof(GetById), "self", HttpMethods.Get, new { id, fields }),
                linkService.Create(nameof(Update), "update", HttpMethods.Put, new { id }),
                linkService.Create(nameof(Patch), "partial-update", HttpMethods.Patch, new { id }),
                linkService.Create(nameof(Delete), "delete", HttpMethods.Delete, new { id }),
                linkService.Create(
                    nameof(HabitTagsController.UpsertHabitTags),
                    "upsert-tags",
                    HttpMethods.Put,
                    new { habitId = id },
                    HabitTagsController.Name)
            ];
        }
    }
}
