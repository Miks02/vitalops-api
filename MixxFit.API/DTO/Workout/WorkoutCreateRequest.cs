using MixxFit.API.DTO.ExerciseEntry;

namespace MixxFit.API.DTO.Workout;

public class WorkoutCreateRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime WorkoutDate { get; set; }
    public List<ExerciseEntryDto> ExerciseEntries { get; set; } = [];
}