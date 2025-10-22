namespace HabitTracker.Api.Settings
{
    public sealed class CorsOptions
    {
        public const string PolicyName = "HabitTrackerCorsPolicy";
        public const string SectionName = "Cors";

        public required string[] AllowedOrigins { get; init; }
    }
}
