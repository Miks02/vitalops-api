using FluentValidation;
using WorkoutTrackerApi.DTO.Workout;

namespace WorkoutTrackerApi.Validators;

public class WorkoutCreateRequestValidator : AbstractValidator<WorkoutCreateRequest>
{
    public WorkoutCreateRequestValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Workout name is required")
            .MaximumLength(200).WithMessage("Name is too long (200 characters max)");

        RuleFor(p => p.WorkoutDate)
            .NotEmpty().WithMessage("Workout date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Workout date cannot be in the future");

        RuleFor(p => p.Notes)
            .MaximumLength(500).WithMessage("Notes are too long (500 characters max)")
            .When(p => !string.IsNullOrEmpty(p.Notes));

        RuleFor(p => p.ExerciseEntries)
            .NotEmpty().WithMessage("Exercise entries are required")
            .Must(entries => entries.Count <= 100).WithMessage("Maximum 100 exercise entries allowed");
        

        RuleForEach(p => p.ExerciseEntries)
            .SetValidator(new ExerciseEntryDtoValidator());

    }
}