using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitalOps.API.DTO.Weight;
using VitalOps.API.Extensions;
using VitalOps.API.Services.Interfaces;

namespace VitalOps.API.Controllers
{
    [Authorize]
    [Route("api/weight-entries")]
    [ApiController]
    public class WeightEntryController : BaseController
    {
        private readonly IWeightEntryService _weightService;

        public WeightEntryController(IWeightEntryService weightService)
        {
            _weightService = weightService;
        }

        [HttpGet]
        public async Task<ActionResult<WeightSummaryDto?>> GetMyWeightSummary(int? year = null, int? month = null, CancellationToken cancellationToken = default)
        {
            return await _weightService.GetUserWeightSummaryAsync(CurrentUserId, year, month, cancellationToken);
        }

        [HttpGet("logs")]
        public async Task<ActionResult> GetMyWeightLogs(int? month = null, int? year = null, CancellationToken cancellationToken = default)
        {
            var logs = await _weightService.GetUserWeightLogsAsync(CurrentUserId, month, year, cancellationToken);

            return Ok(logs);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<WeightEntryDetailsDto?>> GetMyWeightLog([FromRoute] int id)
        {

            return await _weightService.GetUserWeightEntryByIdAsync(CurrentUserId, id);
        }

        [HttpPost]
        public async Task<ActionResult> AddWeightEntry([FromBody] WeightCreateRequestDto request, CancellationToken cancellationToken = default)
        {
            var result = await _weightService.AddWeightEntryAsync(request, CurrentUserId, cancellationToken);

            return result.ToActionResult();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteWeightEntry([FromRoute] int id, CancellationToken cancellationToken = default)
        {
            var result = await _weightService.DeleteEntryAsync(id, CurrentUserId, cancellationToken);

            return result.ToActionResult();
        }

    }
}
