using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WorkoutTrackerApi.Data;
using WorkoutTrackerApi.DTO.ExerciseEntry;
using WorkoutTrackerApi.DTO.Global;
using WorkoutTrackerApi.DTO.SetEntry;
using WorkoutTrackerApi.DTO.Workout;
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

    public async Task<ServiceResult<PagedResult<WorkoutDto>>> GetUserWorkoutsPagedAsync(QueryParams queryParams, string userId, CancellationToken cancellationToken = default)
    {

        var query = QueryBuilder(queryParams, userId);
        
        IQueryable<WorkoutDto> finalQuery = query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(ProjectToWorkoutDto());

        int totalWorkouts = await query.CountAsync(cancellationToken);
        var pagedWorkouts = await finalQuery.ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<WorkoutDto>(pagedWorkouts, queryParams.Page, queryParams.PageSize, totalWorkouts);

        return ServiceResult<PagedResult<WorkoutDto>>.Success(pagedResult);

    }

    public async Task<ServiceResult<WorkoutDto>> GetWorkoutByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var workout = await _context.Workouts
            .Where(w => w.Id == id)
            .AsNoTracking()
            .Select(ProjectToWorkoutDto())
            .FirstOrDefaultAsync(cancellationToken);

        if (workout is null)
        {
            LogInformation($"Workout with id {id} not found");
            return ServiceResult<WorkoutDto>.Failure(Error.Resource.NotFound("Workout"));
        }
        
        return ServiceResult<WorkoutDto>.Success(workout);
    }
    
    public async Task<ServiceResult<WorkoutDto>> AddWorkoutAsync(WorkoutCreateRequest request, CancellationToken cancellationToken = default)
    {

        var newWorkout = new Workout()
        {
            Name = request.Name,
            Notes = request.Notes,
            UserId = request.UserId,
            ExerciseEntries = request.ExerciseEntries.Select(e => new ExerciseEntry()
            {
                Name = e.Name,
                ExerciseType = e.ExerciseType,
                DistanceKm = e.DistanceKm,
                Duration = e.Duration,
                AvgHeartRate = e.AvgHeartRate,
                CaloriesBurned = e.CaloriesBurned,
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
            return ServiceResult<WorkoutDto>.Failure(Error.Database.SaveChangesFailed());
        }
        

        LogInformation("Workout has been added successfully");

        var workoutDto = MapToWorkoutDto().Invoke(newWorkout);
        
        return ServiceResult<WorkoutDto>.Success(workoutDto);
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
            return ServiceResult<WorkoutDto>.Failure(Error.Database.SaveChangesFailed());
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
 
    private static Expression<Func<Workout, WorkoutDto>> ProjectToWorkoutDto()
    {
        return w => new WorkoutDto()
        {
            Id = w.Id,
            Name = w.Name,
            Notes = w.Notes,
            UserId = w.UserId,
            UserName = w.User.UserName,
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
                })
            })
        };

    }

    private static Func<Workout, WorkoutDto> MapToWorkoutDto()
    {
        return w => new WorkoutDto
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