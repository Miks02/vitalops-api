using FluentValidation;
using WorkoutTrackerApi.DTO.Auth;
using WorkoutTrackerApi.Models;

namespace WorkoutTrackerApi.Validators;

public class RegisterValidator : AbstractValidator<RegisterRequestDto>
{

    public RegisterValidator()
    {
        RuleFor(r => r.FirstName)
            .NotEmpty()
            .WithMessage("First name is required.")
            .MinimumLength(2)
            .WithMessage("Minimum length for first name is 2 characters")
            .MaximumLength(20)
            .WithMessage("Maximum length for first name is 20 characters");
        
        RuleFor(r => r.LastName)
            .NotEmpty()
            .WithMessage("Last name is required.")
            .MinimumLength(2)
            .WithMessage("Minimum length for last name is 2 characters.")
            .MaximumLength(20)
            .WithMessage("Maximum length for last name is 20 characters.");

        RuleFor(r => r.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Enter a valid email address.");
        
        RuleFor(r => r.UserName)
            .NotEmpty()
            .WithMessage("Username is required.")
            .MinimumLength(4)
            .WithMessage("Minimum length for username is 4 characters")
            .MaximumLength(20)
            .WithMessage("Maximum length for username is 20 characters");

        RuleFor(r => r.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(6)
            .WithMessage("Password needs to be at least 6 characters long");

        RuleFor(r => r.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Confirm your password.")
            .Equal(r => r.Password)
            .WithMessage("Passwords don't match");

    }
    
}