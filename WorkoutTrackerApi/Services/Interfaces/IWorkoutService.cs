using WorkoutTrackerApi.DTO.Global;
using WorkoutTrackerApi.DTO.Workout;
using WorkoutTrackerApi.Services.Results;

namespace WorkoutTrackerApi.Services.Interfaces;

public interface IWorkoutService
{
    Task<ServiceResult<WorkoutPageDto>> GetUserWorkoutsPagedAsync(QueryParams queryParams, string userId, CancellationToken cancellationToken = default);

    Task<ServiceResult<PagedResult<WorkoutListItemDto>>> GetUserWorkoutsByQueryParams(QueryParams queryParams, string userId, CancellationToken cancellationToken = default);
    
    Task<ServiceResult<WorkoutDetailsDto>> GetWorkoutByIdAsync(int id, CancellationToken cancellationToken = default);
    
    Task<ServiceResult<WorkoutDetailsDto>> AddWorkoutAsync(WorkoutCreateRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteWorkoutAsync(int id, string userId, CancellationToken cancellationToken = default);
}