using MixxFit.API.DTO.SetEntry;
using MixxFit.API.Enums;

namespace MixxFit.API.DTO.ExerciseEntry;

public class ExerciseEntryDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public ExerciseType ExerciseType { get; set; }
    public CardioType? CardioType { get; set; }
    
    public double? DistanceKm { get; set; }
    public int? DurationMinutes { get; set; }
    public int? DurationSeconds { get; set; }
    public int? AvgHeartRate { get; set; }
    public int? MaxHeartRate { get; set; }

    public double? CaloriesBurned { get; set; }

    public double? PaceMinPerKm { get; set; }

    public int? WorkIntervalSec { get; set; }
    public int? RestIntervalSec { get; set; }
    public int? IntervalsCount { get; set; }
    
    public IEnumerable<SetEntryDto> Sets { get; set; } = [];
}