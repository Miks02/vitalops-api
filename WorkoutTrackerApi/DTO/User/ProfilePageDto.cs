using WorkoutTrackerApi.DTO.Workout;

namespace WorkoutTrackerApi.DTO.User
{
    public class ProfilePageDto
    {
        public IReadOnlyList<WorkoutListItemDto> RecentWorkouts { get; set; } = [];
        public int? WorkoutStreak { get; set; }
        public int? DailyCalorieGoal { get; set; }

    }
}
