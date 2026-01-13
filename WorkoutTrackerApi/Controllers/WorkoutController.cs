using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerApi.DTO.Global;
using WorkoutTrackerApi.DTO.Workout;
using WorkoutTrackerApi.Extensions;
using WorkoutTrackerApi.Services.Interfaces;

namespace WorkoutTrackerApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WorkoutController : ControllerBase
    {
        private readonly IWorkoutService _workoutService;
        
        public WorkoutController(IWorkoutService workoutService)
        {
            _workoutService = workoutService;
        }


        [HttpGet("overview")]
        public async Task<IActionResult> GetMyWorkoutsPage(
            [FromQuery] string sortBy = "newest", 
            [FromQuery] string searchBy = "", 
            [FromQuery] DateTime? date = null, 
            [FromQuery] int page = 1 )
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var queryParams = new QueryParams(page, searchBy, sortBy, date);

            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("An error occurred. User ID is null");
            }
            
            var getWorkoutsResult = await _workoutService.GetUserWorkoutsPagedAsync(queryParams, userId);

            return getWorkoutsResult.ToIActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetMyWorkoutsByQueryParams(
            [FromQuery] string sortBy = "newest", 
            [FromQuery] string searchBy = "", 
            [FromQuery] DateTime? date = null, 
            [FromQuery] int page = 1)
        {
            var queryParams = new QueryParams(page, searchBy, sortBy, date);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var getWorkoutsResult = await _workoutService.GetUserWorkoutsByQueryParamsAsync(queryParams, userId!);

            return getWorkoutsResult.ToIActionResult();
        }

        [HttpGet("workout/{id:int}")]
        public async Task<IActionResult> GetWorkout([FromRoute] int id)
        {
            var workouts = await _workoutService.GetWorkoutByIdAsync(id);
            
            return workouts.ToIActionResult();
        }

        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> DeleteWorkout([FromRoute] int id)
        {
            var workoutDeleteResult = await _workoutService.DeleteWorkoutAsync(id, User.FindFirstValue(ClaimTypes.NameIdentifier)!);



            return workoutDeleteResult.ToIActionResult();

        }

        [HttpPost]
        public async Task<IActionResult> AddWorkout([FromBody] WorkoutCreateRequest request)
        {

            request.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            
            var addResult = await _workoutService.AddWorkoutAsync(request);


            return addResult.ToIActionResult();
        }
        
    }
}
