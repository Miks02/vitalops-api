using WorkoutTrackerApi.DTO.Dashboard;

namespace WorkoutTrackerApi.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> LoadDashboardAsync(string userId, CancellationToken cancellationToken);
    }
}
