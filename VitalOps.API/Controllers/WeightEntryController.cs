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
        public async Task<ActionResult<WeightSummaryDto?>> GetMyWeightSummary(int? year = null, int? month = null, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            return await _weightService.GetUserWeightSummaryAsync(userId, year, month, cancellationToken);
        }

        [HttpGet("logs")]
        public async Task<ActionResult> GetMyWeightLogs(int? month = null, int? year = null, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                             
            var logs = await _weightService.GetUserWeightLogsAsync(userId, month, year, cancellationToken);

            return Ok(logs);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<WeightEntryDetailsDto?>> GetMyWeightLog([FromRoute] int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            return await _weightService.GetUserWeightEntryByIdAsync(userId, id);
        }

        [HttpPost]
        public async Task<ActionResult> AddWeightEntry([FromBody] WeightCreateRequestDto request, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _weightService.AddWeightEntryAsync(request, userId, cancellationToken);

            return result.ToActionResult();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteWeightEntry([FromRoute] int id, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _weightService.DeleteEntryAsync(id, userId, cancellationToken);

            return result.ToActionResult();
        }

    }
}
