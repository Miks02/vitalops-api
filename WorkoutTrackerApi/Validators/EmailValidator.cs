using FluentValidation;
using WorkoutTrackerApi.DTO.User;

namespace WorkoutTrackerApi.Validators
{
    public class EmailValidator : AbstractValidator<UpdateEmailDto>
    {
        public EmailValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email address");
                
        }
    }
}
