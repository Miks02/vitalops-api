using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VitalOps.API.Data;
using VitalOps.API.DTO.ExerciseEntry;
using VitalOps.API.DTO.Global;
using VitalOps.API.DTO.SetEntry;
using VitalOps.API.DTO.Workout;
using VitalOps.API.Enums;
using VitalOps.API.Models;
using VitalOps.API.Services.Interfaces;
using VitalOps.API.Services.Results;
using VitalOps.API.Extensions;

namespace VitalOps.API.Services.Implementations;

public class WorkoutService : IWorkoutService
{
    private readonly ILogger<WorkoutService> _logger;
    private readonly AppDbContext _context;
    private readonly int _pageSize = 8;

    public WorkoutService
        (   
            ILogger<WorkoutService> logger,
            AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<WorkoutPageDto> GetUserWorkoutsPagedAsync(QueryParams queryParams, string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            throw new InvalidOperationException("CRITICAL ERROR: user id is null or empty");

        var query = QueryBuilder(queryParams, userId);
        
        int totalPaginatedWorkouts = await query.CountAsync(cancellationToken);
        int totalWorkouts = await CountWorkouts(queryParams, userId);

        var pagedWorkouts = await query.ToListAsync(cancellationToken);
        var workoutSummary = await BuildWorkoutSummary(userId);

        var pagedResult = new PagedResult<WorkoutListItemDto>(pagedWorkouts, queryParams.Page, _pageSize, totalPaginatedWorkouts, totalWorkouts);

        var workoutPage = new WorkoutPageDto
        {
            PagedWorkouts = pagedResult,
            WorkoutSummary = workoutSummary
        };

        return workoutPage;
    }

    public async Task<PagedResult<WorkoutListItemDto>> GetUserWorkoutsByQueryParamsAsync(QueryParams queryParams, string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            throw new InvalidOperationException("CRITICAL ERROR: user id is null or empty");

        var query = QueryBuilder(queryParams, userId);

        var paginatedWorkouts = await query.ToListAsync(cancellationToken);

        var totalPaginatedWorkouts = await query.CountAsync(cancellationToken);

        var totalWorkouts = await CountWorkouts(queryParams, userId);

        return new PagedResult<WorkoutListItemDto>(paginatedWorkouts, queryParams.Page, _pageSize, totalPaginatedWorkouts, totalWorkouts); 
    }

    public async Task<IReadOnlyList<WorkoutListItemDto>> GetRecentWorkoutsAsync(string userId, int itemsToTake, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            throw new InvalidOperationException("CRITICAL ERROR: user id is null or empty");

        if (itemsToTake <= 0)
        {
            _logger.LogError("Items to take must be greater than zero");
            throw new ArgumentOutOfRangeException(nameof(itemsToTake), "Items to take must be greater than zero");
        }

        var recentWorkouts = await _context.Workouts
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.WorkoutDate)
            .Take(itemsToTake)
            .Select(ProjectToWorkoutListItemDto())
            .ToListAsync(cancellationToken);

        return recentWorkouts;

    }

    public async Task<Result<WorkoutDetailsDto>> GetWorkoutByIdAsync(int id, string? userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            throw new InvalidOperationException("CRITICAL ERROR: user id is null or empty");

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

        var workoutDto = MapToWorkoutDetailsDto().Invoke(workout);

        return Result<WorkoutDetailsDto>.Success(workoutDto);
    }

    public async Task<DateTime?> GetLastUserWorkoutAsync(string userId, CancellationToken cancellationToken = default)
    {
        var lastWorkout = await _context.Workouts
            .AsNoTracking()
            .OrderByDescending(w => w.WorkoutDate)
            .Where(w => w.UserId == userId)
            .Select(w => w.WorkoutDate)
            .FirstOrDefaultAsync(cancellationToken);

        return lastWorkout;
    }

    public async Task<int?> CalculateWorkoutStreakAsync(string userId, CancellationToken cancellationToken = default)
    {

        var workoutDates = await _context.Workouts
            .AsNoTracking()
            .OrderByDescending(w => w.WorkoutDate)
            .Where(w => w.UserId == userId)
            .Select(w => w.WorkoutDate)
            .ToListAsync(cancellationToken);

        if (workoutDates.Count == 0)
            return null;

        int streak = 0;

        DateTime currentDate = DateTime.UtcNow;

        foreach (var date in workoutDates)
        {
            if (date.Day != currentDate.Day)
                continue;

            streak++;
            currentDate = currentDate.AddDays(-1);
        }

        return streak;

    }

    public async Task<Result<WorkoutDetailsDto>> AddWorkoutAsync(WorkoutCreateRequest request, string? userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            throw new InvalidOperationException("CRITICAL ERROR: User id is null or empty");

        var newWorkout = new Workout()
        {
            Name = request.Name,
            Notes = request.Notes,
            UserId = userId,
            WorkoutDate = request.WorkoutDate,
            ExerciseEntries = request.ExerciseEntries.Select(e => new ExerciseEntry()
            {
                Name = e.Name,
                ExerciseType = e.ExerciseType,
                CardioType = e.CardioType,
                DistanceKm = e.DistanceKm,
                Duration = ValidateMinutesAndSeconds(e.DurationMinutes, e.DurationSeconds),
                AvgHeartRate = e.AvgHeartRate,
                MaxHeartRate = e.MaxHeartRate,
                CaloriesBurned = e.CaloriesBurned,
                PaceMinPerKm = e.PaceMinPerKm,
                WorkIntervalSec = e.WorkIntervalSec,
                RestIntervalSec = e.RestIntervalSec,
                IntervalsCount = e.IntervalsCount,
                Sets = e.Sets.Select(s => new SetEntry()
                {
                    Reps = s.Reps,
                    WeightKg = s.WeightKg
                }).ToList()
            }).ToList()
        };

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

        var workoutDto = MapToWorkoutDetailsDto().Invoke(newWorkout);

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
            _logger.LogError("CRITICAL: Error happened deleting workout from the database \n {message}", ex.Message);
            return Result<WorkoutDetailsDto>.Failure(Error.Database.SaveChangesFailed());
        }
        
        _logger.LogInformation("Workout deleted successfully");
        return Result.Success();
    }
    
    private IQueryable<WorkoutListItemDto> QueryBuilder(QueryParams queryParams, string? userId = "")
    {
        var query = _context.Workouts
            .AsNoTracking();
            
        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(w => w.UserId == userId);

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

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            string searchPattern = $"%{queryParams.Search}%";
            query = query.Where(w => EF.Functions.Like(w.Name, searchPattern));
        }

        if (queryParams.Date is not null)
        {
            query = query.Where(w => w.WorkoutDate == queryParams.Date);
        }

        query = query
            .Skip((queryParams.Page - 1) * _pageSize)
            .Take(_pageSize);

        return query.Select(ProjectToWorkoutListItemDto());

    }

    private async Task<int> CountWorkouts(QueryParams? queryParams = null, string userId = "")
    {
        var query = _context.Workouts
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(w => w.UserId == userId);

        if (queryParams is null)
            return await query
                .Select(w => w.Id)
                .CountAsync();

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            string searchPattern = $"%{queryParams.Search}%";
            query = query.Where(w => EF.Functions.Like(w.Name, searchPattern));
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

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            string searchPattern = $"%{queryParams.Search}%";
            query = query.Where(w => EF.Functions.Like(w.Name, searchPattern));
        }

        if (queryParams.Date is not null)
        {
            query = query.Where(w => w.WorkoutDate == queryParams.Date);
        }

        return await query.Select(ProjectToWorkoutListItemDto()).CountAsync();

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
 
    private static Expression<Func<Workout, WorkoutListItemDto>> ProjectToWorkoutListItemDto()
    {
        return w => new WorkoutListItemDto()
        {
            Id = w.Id,
            Name = w.Name,
            ExerciseCount = w.ExerciseEntries.Count,
            SetCount = w.ExerciseEntries.Sum(e => e.Sets.Count),
            WorkoutDate = w.WorkoutDate,
            HasCardio = w.ExerciseEntries.Any(e => e.ExerciseType == ExerciseType.Cardio),
            HasWeights = w.ExerciseEntries.Any(e => e.ExerciseType == ExerciseType.WeightLifting),
            HasBodyWeight = w.ExerciseEntries.Any(e => e.ExerciseType == ExerciseType.BodyWeight)
        };

    }

    private static Func<Workout, WorkoutDetailsDto> MapToWorkoutDetailsDto()
    {
        return w => new WorkoutDetailsDto
        {
            Id = w.Id,
            Name = w.Name,
            Notes = w.Notes,
            UserId = w.UserId,
            CreatedAt = w.CreatedAt,
            WorkoutDate = w.WorkoutDate,
            Exercises = w.ExerciseEntries.Select(e => new ExerciseEntryDto()
            {
                Id = e.Id,
                Name = e.Name,
                ExerciseType = e.ExerciseType,
                AvgHeartRate = e.AvgHeartRate,
                CaloriesBurned = e.CaloriesBurned,
                DistanceKm = e.DistanceKm,
                DurationMinutes = e.Duration.ToIntegerFromNullableMinutes(),
                DurationSeconds = e.Duration.ToIntegerFromNullableSeconds(),
                Sets = e.Sets.Select(s => new SetEntryDto()
                {
                    Reps = s.Reps,
                    WeightKg = s.WeightKg
                }).ToList()
            }).ToList()
        };
    }

    private static TimeSpan? ValidateMinutesAndSeconds(int? minutes, int? seconds)
    {
        if (minutes is null || seconds is null)
            return null;

        TimeSpan fromMinutes = TimeSpan.FromMinutes((double)minutes);
        TimeSpan fromSeconds = TimeSpan.FromSeconds((double)seconds);

        return fromMinutes + fromSeconds;
    }
    
}