using FluentValidation;
using HabitTracker.Api.Database;
using HabitTracker.Api.DTOs.Habits;
using HabitTracker.Api.Entities;
using HabitTracker.Api.Extensions;
using HabitTracker.Api.Middleware;
using HabitTracker.Api.Services;
using HabitTracker.Api.Services.Sorting;
using HabitTracker.Api.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.RateLimiting;

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

            builder.Services.AddDbContext<AppIdentityDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Database"), sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Identity);
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

            builder.Services.AddTransient<TokenProvider>();

            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<UserContext>();

            builder.Services.AddScoped<GithubAccessTokenService>();
            builder.Services.AddTransient<GithubService>();
            builder.Services
                .AddHttpClient("github")
                .ConfigureHttpClient(httpClient =>
                {
                    httpClient.BaseAddress = new Uri("https://api.github.com");

                    httpClient.DefaultRequestHeaders
                        .UserAgent.Add(new ProductInfoHeaderValue("HabitTracker", "1.0"));

                    httpClient.DefaultRequestHeaders
                        .Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
                });

            return builder;
        }

        public static WebApplicationBuilder AddAuthenticationServices(this WebApplicationBuilder builder)
        {
            builder.Services
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>();

            builder.Services.Configure<JwtAuthOptions>(builder.Configuration.GetSection("Jwt"));

            JwtAuthOptions jwtAuthOptions = builder.Configuration.GetSection("Jwt").Get<JwtAuthOptions>()!;

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = jwtAuthOptions.Issuer,
                        ValidAudience = jwtAuthOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthOptions.Key))
                    };
                });

            builder.Services.AddAuthorization();

            return builder;
        }

        public static WebApplicationBuilder AddCorsPolicy(this WebApplicationBuilder builder)
        {
            CorsOptions corsOptions = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()!;

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(CorsOptions.PolicyName, policy =>
                {
                    policy
                        .WithOrigins(corsOptions.AllowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            return builder;
        }

        public static WebApplicationBuilder AddRateLimiting(this WebApplicationBuilder builder)
        {
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, token) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter = $"{retryAfter.TotalSeconds}";

                        ProblemDetailsFactory problemDetailsFactory = context.HttpContext.RequestServices
                            .GetRequiredService<ProblemDetailsFactory>();

                        ProblemDetails problemDetails = problemDetailsFactory.CreateProblemDetails(
                            context.HttpContext,
                            StatusCodes.Status429TooManyRequests,
                            "Too Many Requests",
                            detail: $"Too many requests. Please try again after {retryAfter.TotalSeconds} seconds.");

                        await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, token);
                    }
                };

                options.AddPolicy("default", httpContext =>
                {
                    string identityId = httpContext.User.GetIdentityId() ?? string.Empty;

                    if (!string.IsNullOrEmpty(identityId))
                    {
                        return RateLimitPartition.GetTokenBucketLimiter(identityId, _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 100,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 5,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                            TokensPerPeriod = 25
                        });
                    }

                    return RateLimitPartition.GetFixedWindowLimiter("anonymous", _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1)
                    });
                });
            });

            return builder;
        }
    }
}
