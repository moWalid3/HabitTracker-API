using HabitTracker.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HabitTracker.Api.Database.Configurations
{
    public sealed class HabitTagConfiguration : IEntityTypeConfiguration<HabitTag>
    {
        public void Configure(EntityTypeBuilder<HabitTag> builder)
        {
            builder.HasKey(ht => new { ht.HabitId, ht.TagId });

            builder.HasOne<Habit>()
                .WithMany(h => h.HabitTags)
                .HasForeignKey(ht => ht.HabitId);

            builder.HasOne<Tag>()
                .WithMany()
                .HasForeignKey(ht => ht.TagId);

            builder.Property(ht => ht.HabitId).HasMaxLength(500);
            builder.Property(ht => ht.TagId).HasMaxLength(500);
        }
    }
}
