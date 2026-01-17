using WorkoutTrackerApi.Data;
using WorkoutTrackerApi.DTO.Dashboard;
using WorkoutTrackerApi.Services.Interfaces;

namespace WorkoutTrackerApi.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IWorkoutService _workoutService;

        public DashboardService(IWorkoutService workoutService)
        {
            _workoutService = workoutService;
        }

        public async Task<DashboardDto> LoadDashboardAsync(string userId, CancellationToken cancellationToken = default)
        {

            if (userId is null)
                throw new InvalidOperationException("Failed to load the dashboard, user id is null");

            var lastWorkoutDate = await _workoutService.GetLastUserWorkoutAsync(userId, cancellationToken);
            var recentWorkouts = await _workoutService.GetRecentWorkoutsAsync(userId, 10, cancellationToken);


            return new DashboardDto()
            {
                LastWorkoutDate = lastWorkoutDate,
                RecentWorkouts = recentWorkouts
            };
        }


    }
}
