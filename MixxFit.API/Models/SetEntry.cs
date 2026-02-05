namespace MixxFit.API.Models;

public class SetEntry
{
    public int Id { get; set; }
    
    public int ExerciseEntryId { get; set; }
    public ExerciseEntry ExerciseEntry { get; set; } = null!;
    
    public int Reps { get; set; }
    public double WeightKg { get; set; }
}