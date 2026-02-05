using Humanizer;
using Microsoft.EntityFrameworkCore;
using MixxFit.API.Data;
using MixxFit.API.DTO.Global;
using MixxFit.API.DTO.Workout;
using MixxFit.API.Models;
using MixxFit.API.Services.Interfaces;
using MixxFit.API.Services.Results;
using MixxFit.API.Mappers;

namespace MixxFit.API.Services.Implementations;

public class WorkoutService : IWorkoutService
{
    private readonly ILogger<WorkoutService> _logger;
    private readonly AppDbContext _context;
    private readonly int _pageSize = 8;

    public WorkoutService
            (
            ILogger<WorkoutService> logger,
            AppDbContext context
            )
    {
        _logger = logger;
        _context = context;
    }

    public async Task<WorkoutPageDto> GetUserWorkoutsPagedAsync(QueryParams queryParams, string userId, CancellationToken cancellationToken = default)
    {
        var pagedQuery = BuildPagedWorkoutQuery(queryParams, userId);

        int totalPaginatedWorkouts = await pagedQuery.CountAsync(cancellationToken);
        int totalWorkouts = await CountWorkoutsAsync(queryParams, userId, cancellationToken);

        var pagedWorkouts = await pagedQuery.ToListAsync(cancellationToken);
        var workoutSummary = await BuildWorkoutSummary(userId);

        var pagedResult = new PagedResult<WorkoutListItemDto>(pagedWorkouts, queryParams.Page, _pageSize, totalPaginatedWorkouts, totalWorkouts);

        return new WorkoutPageDto()
        {
            PagedWorkouts = pagedResult,
            WorkoutSummary = workoutSummary
        };
    }

    public async Task<PagedResult<WorkoutListItemDto>> GetUserWorkoutsByQueryParamsAsync(QueryParams queryParams, string userId, CancellationToken cancellationToken = default)
    {
        var pagedQuery = BuildPagedWorkoutQuery(queryParams, userId);

        var paginatedWorkouts = await pagedQuery.ToListAsync(cancellationToken);

        var totalPaginatedWorkouts = await pagedQuery.CountAsync(cancellationToken);

        var totalWorkouts = await CountWorkoutsAsync(queryParams, userId, cancellationToken);

        return new PagedResult<WorkoutListItemDto>(paginatedWorkouts, queryParams.Page, _pageSize, totalPaginatedWorkouts, totalWorkouts);
    }

    public async Task<IReadOnlyList<WorkoutListItemDto>> GetRecentWorkoutsAsync(string userId, int itemsToTake, CancellationToken cancellationToken = default)
    {
        if (itemsToTake <= 0)
        {
            _logger.LogError("Items to take must be greater than zero");
            throw new ArgumentOutOfRangeException(nameof(itemsToTake), "Items to take must be greater than zero");
        }

        return await _context.Workouts
            .AsNoTracking()
            .Include(w => w.ExerciseEntries)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.WorkoutDate)
            .Take(itemsToTake)
            .Select(w => w.ToWorkoutListItemDto())
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<WorkoutDetailsDto>> GetWorkoutByIdAsync(int id, string? userId, CancellationToken cancellationToken = default)
    {
        var workout = await _context.Workouts
            .AsNoTracking()
            .Where(w => w.Id == id && w.UserId == userId)
            .Include(w => w.ExerciseEntries)
            .ThenInclude(e => e.Sets)
            .FirstOrDefaultAsync(cancellationToken);

        if (workout is null)
        {
            _logger.LogInformation("Workout with id {id} not found", id);
            return Result<WorkoutDetailsDto>.Failure(Error.Resource.NotFound("Workout"));
        }

        var workoutDto = workout.ToWorkoutDetailsDto();

        return Result<WorkoutDetailsDto>.Success(workoutDto);
    }

    public async Task<DateTime?> GetLastUserWorkoutAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Workouts
            .AsNoTracking()
            .OrderByDescending(w => w.WorkoutDate)
            .Where(w => w.UserId == userId)
            .Select(w => (DateTime?)w.WorkoutDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<WorkoutsPerMonthDto> GetUserWorkoutCountsByMonthAsync(string userId, int? year)
    {
        var years = await _context.Workouts
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .Select(w => w.WorkoutDate.Year)
            .Distinct()
            .OrderByDescending(w => w)
            .ToListAsync();

        var selectedYear = year ?? DateTime.UtcNow.Year;

        var stats = await _context.Workouts
            .AsNoTracking()
            .Where(w => w.UserId == userId && w.WorkoutDate.Year == selectedYear)
            .GroupBy(w => w.WorkoutDate.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .ToListAsync();

        return new WorkoutsPerMonthDto()
        {
            Years = years,
            JanuaryWorkouts = stats.FirstOrDefault(x => x.Month == 1)?.Count ?? 0,
            FebruaryWorkouts =  stats.FirstOrDefault(x => x.Month == 2)?.Count ?? 0,
            MarchWorkouts =  stats.FirstOrDefault(x => x.Month == 3)?.Count ?? 0,
            AprilWorkouts =  stats.FirstOrDefault(x => x.Month == 4)?.Count ?? 0,
            MayWorkouts =  stats.FirstOrDefault(x => x.Month == 5)?.Count ?? 0,
            JuneWorkouts =  stats.FirstOrDefault(x => x.Month == 6)?.Count ?? 0,
            JulyWorkouts =  stats.FirstOrDefault(x => x.Month == 7)?.Count ?? 0,
            AugustWorkouts =  stats.FirstOrDefault(x => x.Month == 8)?.Count ?? 0,
            SeptemberWorkouts =  stats.FirstOrDefault(x => x.Month == 9)?.Count ?? 0,
            OctoberWorkouts =  stats.FirstOrDefault(x => x.Month == 10)?.Count ?? 0,
            NovemberWorkouts =  stats.FirstOrDefault(x => x.Month == 11)?.Count ?? 0,
            DecemberWorkouts =  stats.FirstOrDefault(x => x.Month == 12)?.Count ?? 0
        };

    }

    public async Task<int?> CalculateWorkoutStreakAsync(string userId, CancellationToken cancellationToken = default)
    {
        var workoutDates = await _context.Workouts
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .Select(w => w.WorkoutDate.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToListAsync(cancellationToken);

        if (workoutDates.Count == 0)
            return null;

        var dateSet = workoutDates.ToHashSet();
        var currentDay = DateTime.UtcNow.Date;
        int streak = 0;

        while (dateSet.Contains(currentDay))
        {
            streak++;
            currentDay = currentDay.AddDays(-1);
        }

        return streak;
    }

    public async Task<Result<WorkoutDetailsDto>> AddWorkoutAsync(
        WorkoutCreateRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {

        var workoutsToday = await _context.Workouts
            .AsNoTracking()
            .Where(w => w.UserId == userId && w.CreatedAt.Date == DateTime.UtcNow.Date)
            .Select(w => w.Id)
            .CountAsync(cancellationToken);

        if (workoutsToday == 5)
        {
            _logger.LogWarning("Creating a workout for user {id} has failed", userId);
            return Result<WorkoutDetailsDto>.Failure(Error.General.LimitReached("Workout limit has been reached"));
        }

        var newWorkout = request.ToWorkoutFromCreateRequest(userId);

        try
        {
            await _context.AddAsync(newWorkout, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError("CRITICAL: Error happened while trying to add workout to the database" + ex.Message);
            return Result<WorkoutDetailsDto>.Failure(Error.Database.SaveChangesFailed());
        }

        _logger.LogInformation("Workout has been added successfully: {workout}", newWorkout);

        var workoutDto = newWorkout.ToWorkoutDetailsDto();

        return Result<WorkoutDetailsDto>.Success(workoutDto);
    }

    public async Task<Result> DeleteWorkoutAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _context.Workouts
                .Where(w => w.Id == id && w.UserId == userId)
                .ExecuteDeleteAsync(cancellationToken);

            if (deleted == 0)
            {
                _logger.LogInformation("Delete failed, workout not found");
                return Result.Failure(Error.Resource.NotFound("Workout"));
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError("CRITICAL: Error happened while deleting workout from the database \n {message}", ex.Message);
            return Result.Failure(Error.Database.SaveChangesFailed());
        }

        _logger.LogInformation("Workout deleted successfully");
        return Result.Success();
    }

    private IQueryable<Workout> BuildWorkoutQuery(QueryParams? queryParams = null, string? userId = "")
    {
        var query = _context.Workouts
            .AsNoTracking()
            .Include(w => w.ExerciseEntries)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(w => w.UserId == userId);

        if (queryParams is not null)
        {
            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                string searchPattern = $"%{queryParams.Search}%";
                query = query.Where(w => EF.Functions.Like(w.Name, searchPattern));
            }

            if (queryParams.Date is not null)
            {
                query = query.Where(w => w.WorkoutDate == queryParams.Date);
            }

            if (!string.IsNullOrWhiteSpace(queryParams.Sort))
            {
                switch (queryParams.Sort)
                {
                    case "newest":
                        query = query.OrderByDescending(w => w.WorkoutDate);
                        break;
                    case "oldest":
                        query = query.OrderBy(w => w.WorkoutDate);
                        break;
                }
            }
        }

        return query;
    }

    private IQueryable<WorkoutListItemDto> BuildPagedWorkoutQuery(QueryParams queryParams, string? userId = "")
    {
        var baseQuery = BuildWorkoutQuery(queryParams, userId);

        var paged = baseQuery
            .Skip((queryParams.Page - 1) * _pageSize)
            .Take(_pageSize);

        return paged.Select(w => w.ToWorkoutListItemDto());
    }

    private async Task<int> CountWorkoutsAsync(QueryParams? queryParams = null, string userId = "", CancellationToken cancellationToken = default)
    {
        var baseQuery = BuildWorkoutQuery(queryParams, userId);

        return await baseQuery
            .Select(w => w.Id)
            .CountAsync(cancellationToken);
    }

    private async Task<WorkoutSummaryDto> BuildWorkoutSummary(string userId)
    {
        DateTime? lastWorkoutDate = await _context.Workouts
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .MaxAsync(w => (DateTime?)w.WorkoutDate);

        var workoutCount = await _context.Workouts
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .Select(w => w.Id)
            .CountAsync();

        var exerciseCount = await _context.Workouts
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .SelectMany(w => w.ExerciseEntries)
            .Select(e => e.Id)
            .CountAsync();

        var favoriteExerciseType = await _context.Workouts
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .SelectMany(w => w.ExerciseEntries)
            .GroupBy(e => e.ExerciseType)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefaultAsync();

        return new WorkoutSummaryDto
        {
            WorkoutCount = workoutCount,
            ExerciseCount = exerciseCount,
            LastWorkoutDate = lastWorkoutDate,
            FavoriteExerciseType = favoriteExerciseType
        };
    }

}