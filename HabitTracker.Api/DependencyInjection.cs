using FluentValidation;
using HabitTracker.Api.Database;
using HabitTracker.Api.DTOs.Habits;
using HabitTracker.Api.Entities;
using HabitTracker.Api.Middleware;
using HabitTracker.Api.Services;
using HabitTracker.Api.Services.Sorting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json.Serialization;

namespace HabitTracker.Api
{
    public static class DependencyInjection
    {
        public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver());

            builder.Services.Configure<MvcOptions>(options =>
            {
                NewtonsoftJsonOutputFormatter formatter = options.OutputFormatters
                    .OfType<NewtonsoftJsonOutputFormatter>()
                    .First();

                formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJson);
            });

            builder.Services.AddOpenApi();

            return builder;
        }

        public static WebApplicationBuilder AddErrorHandling(this WebApplicationBuilder builder)
        {
            builder.Services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
                };
            });

            builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            return builder;
        }

        public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Database"), sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application);
                });
            });

            return builder;
        }

        public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            builder.Services.AddTransient<SortMappingProvider>();
            builder.Services.AddSingleton<ISortMappingDefinition, SortMappingDefinition<HabitDto, Habit>>(
                _ => HabitMappings.SortMapping);

            builder.Services.AddTransient<DataShapingService>();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<LinkService>();

            return builder;
        }
    }
}
