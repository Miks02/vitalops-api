using MixxFit.API.DTO.Dashboard;

namespace MixxFit.API.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> LoadDashboardAsync(string userId, CancellationToken cancellationToken);
    }
}
