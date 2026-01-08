using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Design;
using WorkoutTrackerApi.Data;
using WorkoutTrackerApi.DTO.ExerciseEntry;
using WorkoutTrackerApi.DTO.Global;
using WorkoutTrackerApi.DTO.SetEntry;
using WorkoutTrackerApi.DTO.Workout;
using WorkoutTrackerApi.Models;
using WorkoutTrackerApi.Services.Interfaces;
using WorkoutTrackerApi.Services.Results;
using WorkoutTrackerApi.Extensions;

namespace WorkoutTrackerApi.Services.Implementations;

public class WorkoutService : BaseService<WorkoutService> , IWorkoutService
{
    private readonly AppDbContext _context;
    private readonly int _pageSize = 8;

    public WorkoutService
        (   
            ILogger<WorkoutService> logger,
            AppDbContext context) : base(logger
        )
    {
        _context = context;
    }

    public async Task<ServiceResult<WorkoutPageDto>> GetUserWorkoutsPagedAsync(QueryParams queryParams, string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            throw new InvalidOperationException("CRITICAL ERROR: user id is null or empty");

        var query = QueryBuilder(queryParams, userId);
        
        int totalPaginatedWorkouts = await query.CountAsync();
        int totalWorkouts = await CountWorkouts(queryParams, userId);

        var pagedWorkouts = await query.ToListAsync(cancellationToken);
        var workoutSummary = await BuildWorkoutSummary(userId);

        var pagedResult = new PagedResult<WorkoutListItemDto>(pagedWorkouts, queryParams.Page, _pageSize, totalPaginatedWorkouts, totalWorkouts);

        var workoutPage = new WorkoutPageDto
        {
            PagedWorkouts = pagedResult,
            WorkoutSummary = workoutSummary
        };

        return ServiceResult<WorkoutPageDto>.Success(workoutPage);
    }

    public async Task<ServiceResult<PagedResult<WorkoutListItemDto>>> GetUserWorkoutsByQueryParams(QueryParams queryParams, string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            throw new InvalidOperationException("CRITICAL ERROR: user id is null or empty");

        var query = QueryBuilder(queryParams, userId);

        var paginatedWorkouts = await query.ToListAsync(cancellationToken);


        var totalPaginatedWorkouts = await query.CountAsync();

        var totalWorkouts = await CountWorkouts(queryParams, userId);

        var pagedResult = new PagedResult<WorkoutListItemDto>(paginatedWorkouts, queryParams.Page, _pageSize, totalPaginatedWorkouts, totalWorkouts);


        return ServiceResult<PagedResult<WorkoutListItemDto>>.Success(pagedResult);
    }

    public async Task<ServiceResult<WorkoutDetailsDto>> GetWorkoutByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var workout = await _context.Workouts
            .Where(w => w.Id == id)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (workout is null)
        {
            LogInformation($"Workout with id {id} not found");
            return ServiceResult<WorkoutDetailsDto>.Failure(Error.Resource.NotFound("Workout"));
        }

        var workoutDto = MapToWorkoutDetailsDto().Invoke(workout);

        return ServiceResult<WorkoutDetailsDto>.Success(workoutDto);

    }
    
    public async Task<ServiceResult<WorkoutDetailsDto>> AddWorkoutAsync(WorkoutCreateRequest request, CancellationToken cancellationToken = default)
    {

        var newWorkout = new Workout()
        {
            Name = request.Name,
            Notes = request.Notes,
            UserId = request.UserId,
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
            LogCritical("CRITICAL: Error happened while trying to add workout to the database", ex);
            return ServiceResult<WorkoutDetailsDto>.Failure(Error.Database.SaveChangesFailed());
        }


        LogInformation("Workout has been added successfully: " + newWorkout);

        var workoutDto = MapToWorkoutDetailsDto().Invoke(newWorkout);

        return ServiceResult<WorkoutDetailsDto>.Success(workoutDto);
    }

    public async Task<ServiceResult> DeleteWorkoutAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        
        try
        {
            var deleted = await _context.Workouts
                .Where(w => w.Id == id && w.UserId == userId)
                .ExecuteDeleteAsync(cancellationToken);

            if (deleted == 0)
            {
                LogInformation("Delete failed, workout not found");
                return ServiceResult.Failure(Error.Resource.NotFound("Workout"));
            }
        }
        catch (DbUpdateException ex)
        {
            LogCritical("CRITICAL: Error happened deleting workout from the database", ex);
            return ServiceResult<WorkoutDetailsDto>.Failure(Error.Database.SaveChangesFailed());
        }
        
        LogInformation("Workout deleted successfully");
        return ServiceResult.Success();
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

        query = query
            .Skip((queryParams.Page - 1) * _pageSize)
            .Take(_pageSize);

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            string searchPattern = $"%{queryParams.Search}%";
            query = query.Where(w => EF.Functions.Like(w.Name, searchPattern));
        }

        if (queryParams.Date is not null)
        {
            query = query.Where(w => w.WorkoutDate == queryParams.Date);
        }

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
            SetCount = w.ExerciseEntries.Select(e => e.Sets.Count()).Sum(),
            WorkoutDate = w.WorkoutDate
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
                    Id = s.Id,
                    Reps = s.Reps,
                    WeightKg = s.WeightKg
                }).ToList()
            }).ToList()
        };
    }

    private TimeSpan? ValidateMinutesAndSeconds(int? minutes, int? seconds)
    {
        if (minutes is null || seconds is null)
            return null;

        TimeSpan fromMinutes = TimeSpan.FromMinutes((double)minutes);
        TimeSpan fromSeconds = TimeSpan.FromSeconds((double)seconds);

        return fromMinutes + fromSeconds;
    }
    
}