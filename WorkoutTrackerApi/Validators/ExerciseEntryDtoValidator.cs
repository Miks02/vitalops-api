using FluentValidation;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using WorkoutTrackerApi.DTO.ExerciseEntry;
using WorkoutTrackerApi.Enums;

namespace WorkoutTrackerApi.Validators;

public class ExerciseEntryDtoValidator : AbstractValidator<ExerciseEntryDto>
{
    public ExerciseEntryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ExerciseType)
            .IsInEnum();

        RuleFor(x => x.CardioType)
            .IsInEnum()
            .When(x => x.ExerciseType == ExerciseType.Cardio)
            .WithMessage("CardioType must be valid for cardio exercises");

        RuleFor(x => x.Duration)
            .NotNull()
            .When(x => x.ExerciseType == ExerciseType.Cardio)
            .WithMessage("Duration is required for cardio exercises")
            .Must(duration => duration > TimeSpan.Zero)
            .When(x => x.ExerciseType == ExerciseType.Cardio && x.Duration.HasValue)
            .WithMessage("Duration must be greater than 0");

        RuleFor(x => x.DistanceKm)
            .GreaterThan(0)
            .When(x => x.ExerciseType == ExerciseType.Cardio && x.DistanceKm.HasValue)
            .WithMessage("Distance must be greater than 0 km");

        RuleFor(x => x.DistanceKm)
            .Null()
            .When(x => x.ExerciseType == ExerciseType.WeightLifting)
            .WithMessage("Distance is not applicable for strength exercises");

        RuleFor(x => x.AvgHeartRate)
            .InclusiveBetween(30, 220)
            .When(x => x.AvgHeartRate.HasValue)
            .WithMessage("Average heart rate must be between 30 and 220 bpm");

        RuleFor(x => x.MaxHeartRate)
            .InclusiveBetween(30, 220)
            .When(x => x.MaxHeartRate.HasValue)
            .WithMessage("Maximum heart rate must be between 30 and 220 bpm")
            .GreaterThanOrEqualTo(x => x.AvgHeartRate ?? 0)
            .When(x => x.MaxHeartRate.HasValue && x.AvgHeartRate.HasValue)
            .WithMessage("Maximum heart rate must be greater than or equal to average heart rate");

        RuleFor(x => x.PaceMinPerKm)
            .GreaterThan(0)
            .When(x => x.PaceMinPerKm.HasValue)
            .WithMessage("Pace must be greater than 0")
            .LessThanOrEqualTo(20)
            .When(x => x.PaceMinPerKm.HasValue)
            .WithMessage("Pace must be realistic (max 20 min/km)");

        RuleFor(x => x.CaloriesBurned)
            .GreaterThan(0)
            .When(x => x.CaloriesBurned.HasValue)
            .WithMessage("Calories burned must be greater than 0")
            .LessThanOrEqualTo(10000)
            .When(x => x.CaloriesBurned.HasValue)
            .WithMessage("Calories burned must be less than or equal to 10000");

        RuleFor(x => x.WorkIntervalSec)
            .GreaterThan(0)
            .When(x => x.WorkIntervalSec.HasValue)
            .WithMessage("Work interval must be greater than 0 seconds")
            .LessThanOrEqualTo(3600)
            .When(x => x.WorkIntervalSec.HasValue)
            .WithMessage("Work interval must be less than or equal to 3600 seconds (1 hour)");

        RuleFor(x => x.RestIntervalSec)
            .GreaterThan(0)
            .When(x => x.RestIntervalSec.HasValue)
            .WithMessage("Rest interval must be greater than 0 seconds")
            .LessThanOrEqualTo(3600)
            .When(x => x.RestIntervalSec.HasValue)
            .WithMessage("Rest interval must be less than or equal to 3600 seconds (1 hour)");

        RuleFor(x => x.IntervalsCount)
            .GreaterThan(0)
            .When(x => x.IntervalsCount.HasValue)
            .WithMessage("Intervals count must be greater than 0")
            .LessThanOrEqualTo(1000)
            .When(x => x.IntervalsCount.HasValue)
            .WithMessage("Intervals count must be less than or equal to 1000");

        RuleFor(x => x)
            .Must(BeValidIntervalExercise)
            .When(x => x.WorkIntervalSec.HasValue || x.RestIntervalSec.HasValue || x.IntervalsCount.HasValue)
            .WithMessage("If one interval field is set, all interval fields must be set");

        RuleForEach(x => x.Sets)
            .SetValidator(new SetEntryDtoValidator());
    }

    private static bool BeValidIntervalExercise(ExerciseEntryDto exercise)
    {
        var hasWorkInterval = exercise.WorkIntervalSec.HasValue;
        var hasRestInterval = exercise.RestIntervalSec.HasValue;
        var hasIntervalsCount = exercise.IntervalsCount.HasValue;

        if ((hasWorkInterval || hasRestInterval || hasIntervalsCount))
        {
            return hasWorkInterval && hasRestInterval && hasIntervalsCount;
        }

        return true;
    }
}