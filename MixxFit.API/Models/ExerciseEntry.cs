using MixxFit.API.Enums;

namespace MixxFit.API.Models;

public class ExerciseEntry
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    
    public ExerciseType ExerciseType { get; set; }
    public CardioType? CardioType { get; set; }
    
    public int WorkoutId { get; set; }
    public Workout Workout { get; set; } = null!;
    
    public double? DistanceKm { get; set; }
    public TimeSpan? Duration { get; set; }
    
    public int? AvgHeartRate { get; set; }
    public int? MaxHeartRate { get; set; }
    
    public double? CaloriesBurned { get; set; }
    
    public double? PaceMinPerKm { get; set; }
    
    public int? WorkIntervalSec { get; set; }
    public int? RestIntervalSec { get; set; }
    public int? IntervalsCount { get; set; }
    
    public ICollection<SetEntry> Sets { get; set; } = [];

}