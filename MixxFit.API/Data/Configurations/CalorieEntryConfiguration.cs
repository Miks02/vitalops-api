using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MixxFit.API.Models;

namespace MixxFit.API.Data.Configurations;

public class CalorieEntryConfiguration : IEntityTypeConfiguration<CalorieEntry>
{
    public void Configure(EntityTypeBuilder<CalorieEntry> builder)
    {
        builder.Property(p => p.Description)
            .HasMaxLength(512);
        

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.ToTable(entries => entries.HasCheckConstraint($"CK_{nameof(CalorieEntry)}s_{nameof(CalorieEntry.Calories)}_Positive", "\"Calories\" > 0"));

        builder
            .HasOne(c => c.User)
            .WithMany(u => u.CalorieEntries)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}