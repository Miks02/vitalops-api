namespace MixxFit.API.DTO.Workout;

public class WorkoutListItemDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    
    public DateTime WorkoutDate { get; set; }
    
    public int ExerciseCount { get; set; }
     
    public int SetCount { get; set; }

    public bool HasCardio { get; set; }
    public bool HasWeights { get; set; }
    public bool HasBodyWeight { get; set; }
}