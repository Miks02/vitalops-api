using WorkoutTrackerApi.DTO.Global;

namespace WorkoutTrackerApi.DTO.Workout
{
    public class WorkoutPageDto
    {
        public PagedResult<WorkoutListItemDto> PagedWorkouts { get; set; } = null!;

        public WorkoutSummaryDto WorkoutSummary { get; set; } = null!;
    }
}
