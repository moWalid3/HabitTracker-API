using HabitTracker.Api.Settings;

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
                .AddCorsPolicy();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseExceptionHandler();

            app.UseCors(CorsOptions.PolicyName);

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
