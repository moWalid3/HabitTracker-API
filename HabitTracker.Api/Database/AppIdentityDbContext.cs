using HabitTracker.Api.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Database
{
    public sealed class AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
        : IdentityDbContext(options)
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema(Schemas.Identity);

            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.UserId).HasMaxLength(450);
                entity.Property(r => r.Token).HasMaxLength(1000);

                entity.HasIndex(r => r.Token).IsUnique();

                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
