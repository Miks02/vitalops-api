using FluentValidation;
using MixxFit.API.DTO.User;

namespace MixxFit.API.Validators
{
    public class UsernameValidator : AbstractValidator<UpdateUserNameDto>
    {
        public UsernameValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage("Username is required.")
                .MinimumLength(4)
                .WithMessage("At least 2 characters are required")
                .MaximumLength(20)
                .WithMessage("Username cannot exceed 20 characters");
        }
    }
}
