using HabitTracker.Api.Settings;
using Scalar.AspNetCore;

namespace HabitTracker.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder
                .AddApiServices()
                .AddErrorHandling()
                .AddDatabase()
                .AddApplicationServices()
                .AddAuthenticationServices()
                .AddCorsPolicy()
                .AddRateLimiting();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();

                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseExceptionHandler();

            app.UseCors(CorsOptions.PolicyName);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRateLimiter();

            app.MapControllers();

            app.Run();
        }
    }
}
