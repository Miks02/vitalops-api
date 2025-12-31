using WorkoutTrackerApi.DTO.Global;
using WorkoutTrackerApi.DTO.Workout;
using WorkoutTrackerApi.Services.Results;

namespace WorkoutTrackerApi.Services.Interfaces;

public interface IWorkoutService
{
    Task<ServiceResult<PagedResult<WorkoutDto>>> GetUserWorkoutsPagedAsync(QueryParams queryParams, string userId, CancellationToken cancellationToken = default);

    Task<ServiceResult<WorkoutDto>> GetWorkoutByIdAsync(int id, CancellationToken cancellationToken = default);
    
    Task<ServiceResult<WorkoutDto>> AddWorkoutAsync(WorkoutCreateRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteWorkoutAsync(int id, string userId, CancellationToken cancellationToken = default);
}