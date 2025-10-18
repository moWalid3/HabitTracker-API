using HabitTracker.Api.Database.Configurations;
using HabitTracker.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Database
{
    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Habit> Habits { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schemas.Application);

            modelBuilder.ApplyConfiguration(new HabitConfiguration());
            modelBuilder.ApplyConfiguration(new TagConfiguration());
        }
    }
}
