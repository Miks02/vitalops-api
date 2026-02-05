using MixxFit.API.DTO.Workout;

namespace MixxFit.API.DTO.Dashboard
{
    public class DashboardDto
    {
        public double DailyCalories { get; set; }
        public double WaterIntake { get; set; }
        public double AverageSleep { get; set; }
        public DateTime? LastWorkoutDate { get; set; }
        public IReadOnlyList<WorkoutListItemDto> RecentWorkouts { get; set; } = new List<WorkoutListItemDto>();

    }
}
