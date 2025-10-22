using HabitTracker.Api.Entities;
using Microsoft.AspNetCore.Identity;
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

            SeedRoles(builder);
        }

        private static void SeedRoles(ModelBuilder builder)
        {
            IdentityRole[] roles = [
                new() {
                  Id = "7fa71bfe-f97c-4f8f-84b4-7a08a89c031a",
                  Name =  Entities.Roles.Admin
                },
                new() {
                  Id = "483b69c4-7363-43ae-9523-068cb6b8e146",
                  Name =  Entities.Roles.Member
                }
            ];

            builder.Entity<IdentityRole>().HasData(roles);
        }
    }
}
