using MixxFit.API.DTO.Global;
using MixxFit.API.DTO.Workout;
using MixxFit.API.Services.Results;

namespace MixxFit.API.Services.Interfaces;

public interface IWorkoutService
{
    Task<WorkoutPageDto> GetUserWorkoutsPagedAsync(QueryParams queryParams, string userId, CancellationToken cancellationToken = default);
    Task<PagedResult<WorkoutListItemDto>> GetUserWorkoutsByQueryParamsAsync(QueryParams queryParams, string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkoutListItemDto>> GetRecentWorkoutsAsync(string userId, int itemsToTake, CancellationToken cancellationToken = default);
    Task<Result<WorkoutDetailsDto>> GetWorkoutByIdAsync(int id, string? userId, CancellationToken cancellationToken = default);
    Task<DateTime?> GetLastUserWorkoutAsync(string userId, CancellationToken cancellationToken = default);
    Task<WorkoutsPerMonthDto> GetUserWorkoutCountsByMonthAsync(string userId, int? year);

    Task<int?> CalculateWorkoutStreakAsync(string userId, CancellationToken cancellationToken = default);
   
    Task<Result<WorkoutDetailsDto>> AddWorkoutAsync(WorkoutCreateRequest request, string userId,
        CancellationToken cancellationToken = default);
    Task<Result> DeleteWorkoutAsync(int id, string userId, CancellationToken cancellationToken = default);
}