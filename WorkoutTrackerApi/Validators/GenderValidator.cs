using FluentValidation;
using WorkoutTrackerApi.DTO.User;

namespace WorkoutTrackerApi.Validators
{
    public class GenderValidator : AbstractValidator<UpdateGenderDto>
    {
        public GenderValidator()
        {
            RuleFor(x => x.Gender)
                .IsInEnum()
                .NotEmpty()
                .WithMessage("Gender is required");
        }
    }
}
