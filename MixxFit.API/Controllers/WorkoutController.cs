using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MixxFit.API.DTO.Global;
using MixxFit.API.DTO.Workout;
using MixxFit.API.Services.Interfaces;
using MixxFit.API.Extensions;

namespace MixxFit.API.Controllers
{
    [Authorize]
    [Route("api/workouts")]
    [ApiController]
    public class WorkoutController : BaseController
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
            var queryParams = new QueryParams(page, searchBy, sortBy, date);
            
            var getWorkoutsResult = await _workoutService.GetUserWorkoutsPagedAsync(queryParams, CurrentUserId);

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

            var workouts = await _workoutService.GetUserWorkoutsByQueryParamsAsync(queryParams, CurrentUserId);

            return Ok(workouts);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetMyWorkout([FromRoute] int id)
        {
            var workout = await _workoutService.GetWorkoutByIdAsync(id, CurrentUserId);
            
            return workout.ToActionResult();
        }

        [HttpGet("workouts-per-month")]
        public async Task<ActionResult<WorkoutsPerMonthDto>> GetWorkoutsPerMonth([FromQuery] int? year)
        {
            return await _workoutService.GetUserWorkoutCountsByMonthAsync(CurrentUserId, year);
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
            var addResult = await _workoutService.AddWorkoutAsync(request, CurrentUserId);

            return addResult.ToActionResult();
        }
        
    }
}
