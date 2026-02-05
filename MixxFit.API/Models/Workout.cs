namespace MixxFit.API.Models;

public class Workout
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public string? Notes { get; set; }

    public string UserId { get; set; } = null!;
    public virtual User User { get; set; } = null!;

    public DateTime WorkoutDate { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<ExerciseEntry> ExerciseEntries { get; set; } = [];

}