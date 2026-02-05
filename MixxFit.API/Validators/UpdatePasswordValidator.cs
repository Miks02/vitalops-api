using FluentValidation;
using MixxFit.API.DTO.Auth;

namespace MixxFit.API.Validators
{
    public class UpdatePasswordValidator : AbstractValidator<UpdatePasswordDto>
    {
        public UpdatePasswordValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithMessage("Current password is required.")
                .MinimumLength(6)
                .WithMessage("Current password must be at least 6 characters long.");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("New password is required.")
                .MinimumLength(6)
                .WithMessage("New password must be at least 6 characters long.")
                .NotEqual(x => x.CurrentPassword)
                .WithMessage("New password must be different from the current password.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage("Please confirm your new password.")
                .Equal(x => x.NewPassword)
                .WithMessage("Passwords do not match.");
        }
    }
}