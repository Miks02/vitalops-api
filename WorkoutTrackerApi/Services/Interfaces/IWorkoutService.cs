using WorkoutTrackerApi.DTO.Global;
using WorkoutTrackerApi.DTO.Workout;
using WorkoutTrackerApi.Services.Results;

namespace WorkoutTrackerApi.Services.Interfaces;

public interface IWorkoutService
{
    Task<WorkoutPageDto> GetUserWorkoutsPagedAsync(QueryParams queryParams, string userId, CancellationToken cancellationToken = default);

    Task<PagedResult<WorkoutListItemDto>> GetUserWorkoutsByQueryParamsAsync(QueryParams queryParams, string userId, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<WorkoutListItemDto>> GetRecentWorkoutsAsync(string userId, int itemsToTake, CancellationToken cancellationToken = default);

    Task<int?> CalculateWorkoutStreakAsync(string userId, CancellationToken cancellationToken = default);

    Task<ServiceResult<WorkoutDetailsDto>> GetWorkoutByIdAsync(int id, CancellationToken cancellationToken = default);
   
    Task<ServiceResult<WorkoutDetailsDto>> AddWorkoutAsync(WorkoutCreateRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteWorkoutAsync(int id, string userId, CancellationToken cancellationToken = default);
}