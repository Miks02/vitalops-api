using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitalOps.API.DTO.Global;
using VitalOps.API.DTO.Workout;
using VitalOps.API.Services.Interfaces;
using VitalOps.API.Extensions;
using VitalOps.API.Models;

namespace VitalOps.API.Controllers
{
    [Authorize]
    [Route("api/workouts")]
    [ApiController]
    public class WorkoutController : ControllerBase
    {
        private readonly IWorkoutService _workoutService;
        
        public WorkoutController(IWorkoutService workoutService)
        {
            _workoutService = workoutService;
        }


        [HttpGet("overview")]
        public async Task<ActionResult<WorkoutPageDto>> GetMyWorkoutsPage(
            [FromQuery] string sortBy = "newest", 
            [FromQuery] string searchBy = "", 
            [FromQuery] DateTime? date = null, 
            [FromQuery] int page = 1 )
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var queryParams = new QueryParams(page, searchBy, sortBy, date);
            
            var getWorkoutsResult = await _workoutService.GetUserWorkoutsPagedAsync(queryParams, userId!);

            return Ok(getWorkoutsResult);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<WorkoutListItemDto>>> GetMyWorkoutsByQueryParams(
            [FromQuery] string sortBy = "newest", 
            [FromQuery] string searchBy = "", 
            [FromQuery] DateTime? date = null, 
            [FromQuery] int page = 1)
        {
            var queryParams = new QueryParams(page, searchBy, sortBy, date);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var workouts = await _workoutService.GetUserWorkoutsByQueryParamsAsync(queryParams, userId!);

            return Ok(workouts);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetMyWorkout([FromRoute] int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var workout = await _workoutService.GetWorkoutByIdAsync(id, userId);
            
            return workout.ToActionResult();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteWorkout([FromRoute] int id)
        {
            var workoutDeleteResult = await _workoutService.DeleteWorkoutAsync(id, User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            return workoutDeleteResult.ToActionResult();
        }

        [HttpPost]
        public async Task<ActionResult> AddWorkout([FromBody] WorkoutCreateRequest request)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            var addResult = await _workoutService.AddWorkoutAsync(request, null);


            return addResult.ToActionResult();
        }
        
    }
}
