using FluentValidation;
using MixxFit.API.DTO.User;

namespace MixxFit.API.Validators
{
    public class DateOfBirthValidator : AbstractValidator<UpdateDateOfBirthDto>
    {
        public DateOfBirthValidator()
        {
            var today = DateTime.UtcNow.Date;

            RuleFor(x => x.DateOfBirth)
                .NotEqual(default(DateTime))
                .WithMessage("Date of birth is required")
                .LessThan(today)
                .WithMessage("Date of birth must be in the past")
                .GreaterThan(new DateTime(1900, 1, 1))
                .WithMessage("Date of birth year is invalid");

        }
    }
}
