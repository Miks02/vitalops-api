using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VitalOps.API.DTO.Weight;
using VitalOps.API.DTO.Workout;
using VitalOps.API.Extensions;
using VitalOps.API.Services.Interfaces;

namespace VitalOps.API.Controllers
{
    [Authorize]
    [Route("api/weight-entries")]
    [ApiController]
    public class WeightEntryController : ControllerBase
    {
        private readonly IWeightEntryService _weightService;

        public WeightEntryController(IWeightEntryService weightService)
        {
            _weightService = weightService;
        }

        [HttpGet]
        public async Task<ActionResult<WeightSummaryDto?>> GetMyWeightSummary(CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            return await _weightService.GetUserWeightSummaryAsync(userId, cancellationToken);
        }

        [HttpPost]
        public async Task<ActionResult> AddWeightEntry([FromBody] WeightCreateRequestDto request, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _weightService.AddWeightEntryAsync(request, userId, cancellationToken);

            return result.ToActionResult();
        }

    }
}
