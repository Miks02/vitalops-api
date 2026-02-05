
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MixxFit.API.DTO.Dashboard;
using MixxFit.API.Services.Interfaces;

namespace MixxFit.API.Controllers
{
    [Authorize]
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardDto>> GetDashboard(CancellationToken cancellationToken = default)
        {
            return await _dashboardService.LoadDashboardAsync(CurrentUserId, cancellationToken);
        }
    }
}
