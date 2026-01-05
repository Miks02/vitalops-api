using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WorkoutTrackerApi.Data;
using WorkoutTrackerApi.DTO.ExerciseEntry;
using WorkoutTrackerApi.DTO.Global;
using WorkoutTrackerApi.DTO.SetEntry;
using WorkoutTrackerApi.DTO.Workout;
using WorkoutTrackerApi.Enums;
using WorkoutTrackerApi.Models;
using WorkoutTrackerApi.Services.Interfaces;
using WorkoutTrackerApi.Services.Results;

namespace WorkoutTrackerApi.Services.Implementations;

public class WorkoutService : BaseService<WorkoutService> , IWorkoutService
{
    private readonly AppDbContext _context;

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

        var query = QueryBuilder(queryParams, userId);
        
        IQueryable<WorkoutListItemDto> finalQuery = query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(ProjectToWorkoutDto());

        int totalWorkouts = await query.CountAsync(cancellationToken);
        var pagedWorkouts = await finalQuery.ToListAsync(cancellationToken);
        var workoutSummary = await BuildWorkoutSummary();

        var pagedResult = new PagedResult<WorkoutListItemDto>(pagedWorkouts, queryParams.Page, queryParams.PageSize, totalWorkouts);

        var workoutPage = new WorkoutPageDto
        {
            PagedWorkouts = pagedResult,
            WorkoutSummary = workoutSummary
        };


        return ServiceResult<WorkoutPageDto>.Success(workoutPage);

    }

    public async Task<ServiceResult<WorkoutDetailsDto>> GetWorkoutByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var workout = await _context.Workouts
            .Where(w => w.Id == id)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (workout is null)
        {
            LogInformation($"Workout with id {id} not found");
            return ServiceResult<WorkoutDetailsDto>.Failure(Error.Resource.NotFound("Workout"));
        }

       var workoutDto = MapToWorkoutDetailsDto().Invoke(workout);

       return ServiceResult<WorkoutDetailsDto>.Success(workoutDto);
       throw new NotImplementedException();
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
                Duration = e.Duration,
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
        

        LogInformation("Workout has been added successfully");

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
    
    private IQueryable<Workout> QueryBuilder(QueryParams queryParams, string userId)
    {
        var query = _context.Workouts
            .OrderByDescending(w => w.CreatedAt)
            .AsNoTracking();
            

        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(w => w.UserId == userId);

        if (!string.IsNullOrWhiteSpace(queryParams.Sort))
        {
            switch (queryParams.Sort)
            {
                case "newest":
                    query = query.OrderByDescending(w => w.CreatedAt);
                    break;
                case "oldest":
                    query = query.OrderBy(w => w.CreatedAt);
                    break;
            }
        }
        
        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            string searchPattern = $"%{queryParams.Search}%";
            query = query.Where(w => EF.Functions.Like(w.Name, searchPattern));
        }

        return query;

    }

    private async Task<WorkoutSummaryDto> BuildWorkoutSummary() 
    {
        var lastWorkoutDate = await _context.Workouts
            .MaxAsync(w => w.WorkoutDate);


        var exerciseCount = await _context.Workouts
            .Select(w => w.ExerciseEntries)
            .CountAsync();

        var favoriteExerciseType = await _context.Workouts
            .SelectMany(w => w.ExerciseEntries)
            .GroupBy(e => e.ExerciseType)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefaultAsync();


        return new WorkoutSummaryDto
        {
            ExerciseCount = exerciseCount,
            LastWorkoutDate = lastWorkoutDate,
            FavoriteExerciseType = favoriteExerciseType
        };

    }
 
    private static Expression<Func<Workout, WorkoutListItemDto>> ProjectToWorkoutDto()
    {
        return w => new WorkoutListItemDto()
        {
            Id = w.Id,
            Name = w.Name,
            ExerciseCount = w.ExerciseEntries.Count,
            SetCount = w.ExerciseEntries.Select(e => e.Sets).Count(),
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
                Duration = e.Duration,
                Sets = e.Sets.Select(s => new SetEntryDto()
                {
                    Id = s.Id,
                    Reps = s.Reps,
                    WeightKg = s.WeightKg
                }).ToList()
            }).ToList()
        };
    }
    
}