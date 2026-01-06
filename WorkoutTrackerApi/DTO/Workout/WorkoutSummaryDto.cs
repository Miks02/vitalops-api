using System;
using WorkoutTrackerApi.Enums;

namespace WorkoutTrackerApi.DTO.Workout;

public class WorkoutSummaryDto
{
    public int ExerciseCount { get; set; }

    public DateTime? LastWorkoutDate { get; set; }

    public ExerciseType FavoriteExerciseType { get; set; }
}