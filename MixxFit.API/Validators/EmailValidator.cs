using FluentValidation;
using MixxFit.API.DTO.User;

namespace MixxFit.API.Validators
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
