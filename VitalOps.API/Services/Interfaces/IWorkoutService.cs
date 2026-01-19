using VitalOps.API.DTO.Global;
using VitalOps.API.DTO.Workout;
using VitalOps.API.Services.Results;

namespace VitalOps.API.Services.Interfaces;

public interface IWorkoutService
{
    Task<WorkoutPageDto> GetUserWorkoutsPagedAsync(QueryParams queryParams, string userId, CancellationToken cancellationToken = default);
    Task<PagedResult<WorkoutListItemDto>> GetUserWorkoutsByQueryParamsAsync(QueryParams queryParams, string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkoutListItemDto>> GetRecentWorkoutsAsync(string userId, int itemsToTake, CancellationToken cancellationToken = default);
    Task<Result<WorkoutDetailsDto>> GetWorkoutByIdAsync(int id, string? userId, CancellationToken cancellationToken = default);
    Task<DateTime?> GetLastUserWorkoutAsync(string userId, CancellationToken cancellationToken = default);

    Task<int?> CalculateWorkoutStreakAsync(string userId, CancellationToken cancellationToken = default);
   
    Task<Result<WorkoutDetailsDto>> AddWorkoutAsync(WorkoutCreateRequest request, string? userId, CancellationToken cancellationToken = default);
    Task<Result> DeleteWorkoutAsync(int id, string userId, CancellationToken cancellationToken = default);
}