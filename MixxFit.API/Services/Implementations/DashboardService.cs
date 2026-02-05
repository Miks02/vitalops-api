using MixxFit.API.DTO.Dashboard;
using MixxFit.API.Services.Interfaces;
using MixxFit.API.Data;

namespace MixxFit.API.Services.Implementations
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
