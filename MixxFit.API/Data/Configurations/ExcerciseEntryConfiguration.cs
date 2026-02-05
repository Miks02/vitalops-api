using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MixxFit.API.Models;

namespace MixxFit.API.Data.Configurations;

public class ExerciseEntryConfiguration : IEntityTypeConfiguration<ExerciseEntry>
{
    public void Configure(EntityTypeBuilder<ExerciseEntry> builder)
    {
        builder.Property(p => p.Name)
            .HasMaxLength(100);

        builder.HasIndex(p => p.Name);

        builder.ToTable(entries => entries.HasCheckConstraint($"CK_{nameof(ExerciseEntry)}s_{nameof(ExerciseEntry.AvgHeartRate)}_Positive", "\"AvgHeartRate\" > 0"));
        builder.ToTable(entries => entries.HasCheckConstraint($"CK_{nameof(ExerciseEntry)}s_{nameof(ExerciseEntry.MaxHeartRate)}_Positive", "\"MaxHeartRate\" > 0"));
        builder.ToTable(entries => entries.HasCheckConstraint($"CK_{nameof(ExerciseEntry)}s_{nameof(ExerciseEntry.MaxHeartRate)}_GreaterThan_AvgHeartRate", "\"MaxHeartRate\" >= \"AvgHeartRate\""));
        builder.ToTable(entries => entries.HasCheckConstraint($"CK_{nameof(ExerciseEntry)}s_{nameof(ExerciseEntry.WorkIntervalSec)}_Positive", "\"WorkIntervalSec\" > 0"));
        builder.ToTable(entries => entries.HasCheckConstraint($"CK_{nameof(ExerciseEntry)}s_{nameof(ExerciseEntry.RestIntervalSec)}_Positive", "\"RestIntervalSec\" > 0"));
        builder.ToTable(entries => entries.HasCheckConstraint($"CK_{nameof(ExerciseEntry)}s_{nameof(ExerciseEntry.IntervalsCount)}_Positive", "\"IntervalsCount\" > 0"));

        builder.Property(p => p.DistanceKm)
            .HasPrecision(5, 2);

        builder.Property(p => p.CaloriesBurned)
            .HasPrecision(7, 2);

        builder.Property(p => p.PaceMinPerKm)
            .HasPrecision(5, 2);

        builder
            .HasMany(e => e.Sets)
            .WithOne(s => s.ExerciseEntry)
            .HasForeignKey(s => s.ExerciseEntryId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}