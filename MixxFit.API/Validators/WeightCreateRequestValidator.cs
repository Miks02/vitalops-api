using FluentValidation;
using MixxFit.API.DTO.Weight;

namespace MixxFit.API.Validators
{
    public class WeightCreateRequestValidator : AbstractValidator<WeightCreateRequestDto>
    {
        public WeightCreateRequestValidator()
        {
            RuleFor(x => x.Weight)
                .NotEmpty()
                .WithMessage("Weight is required")
                .GreaterThan(25)
                .WithMessage("Weight has to be higher than 25 KG")
                .LessThan(400)
                .WithMessage("Weight has to be lower than 400 KG");

            RuleFor(x => x.Notes)
                .MaximumLength(100)
                .WithMessage("Maximum length is 100 characters");



        }
    }
}
