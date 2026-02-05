using FluentValidation;
using MixxFit.API.DTO.User;

namespace MixxFit.API.Validators
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
