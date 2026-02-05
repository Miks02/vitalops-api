using MixxFit.API.DTO.ExerciseEntry;
using MixxFit.API.DTO.SetEntry;
using MixxFit.API.DTO.Workout;
using MixxFit.API.Enums;
using MixxFit.API.Helpers;
using MixxFit.API.Models;
using MixxFit.API.Extensions;

namespace MixxFit.API.Mappers
{
    public static class WorkoutMapper
    {
        public static WorkoutListItemDto ToWorkoutListItemDto(this Workout workout)
        {
            return new WorkoutListItemDto()
            {
                Id = workout.Id,
                Name = workout.Name,
                ExerciseCount = workout.ExerciseEntries.Count,
                SetCount = workout.ExerciseEntries.Sum(e => e.Sets.Count),
                WorkoutDate = workout.WorkoutDate,
                HasCardio = workout.ExerciseEntries.Any(e => e.ExerciseType == ExerciseType.Cardio),
                HasWeights = workout.ExerciseEntries.Any(e => e.ExerciseType == ExerciseType.WeightLifting),
                HasBodyWeight = workout.ExerciseEntries.Any(e => e.ExerciseType == ExerciseType.BodyWeight)
            };
        }

        public static WorkoutDetailsDto ToWorkoutDetailsDto(this Workout workout)
        {
            return new WorkoutDetailsDto()
            {
                Id = workout.Id,
                Name = workout.Name,
                Notes = workout.Notes,
                UserId = workout.UserId,
                CreatedAt = workout.CreatedAt,
                WorkoutDate = workout.WorkoutDate,
                Exercises = workout.ExerciseEntries.Select(e => new ExerciseEntryDto()
                {
                    Id = e.Id,
                    Name = e.Name,
                    ExerciseType = e.ExerciseType,
                    CardioType = e.CardioType,
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

        public static Workout ToWorkoutFromCreateRequest(this WorkoutCreateRequest request, string userId)
        {
            return new Workout()
            {
                Name = request.Name,
                Notes = request.Notes,
                UserId = userId,
                WorkoutDate = request.WorkoutDate.ToUniversalTime(),
                ExerciseEntries = request.ExerciseEntries.Select(e => new ExerciseEntry()
                {
                    Name = e.Name,
                    ExerciseType = e.ExerciseType,
                    CardioType = e.CardioType,
                    DistanceKm = e.DistanceKm,
                    Duration = Utility.ValidateMinutesAndSeconds(e.DurationMinutes, e.DurationSeconds),
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
        }
    }
}
