using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VitalOps.API.Services.Results;
using VitalOps.API.DTO.Global;

namespace VitalOps.API.Extensions;

public static class ResultExtensions
{
    public static Result HandleResult(this Result result, ILogger? logger)
    {
        if (result.IsSucceeded)
            return Result.Success();

        LogErrors(logger, result.Errors);
        return Result.Failure(result.Errors.ToArray());
    }

    public static Result<T> HandleResult<T>(this Result<T> result, ILogger? logger)
    {
        if (result.IsSucceeded)
            return result.Payload is null ? Result<T>.Success() : Result<T>.Success(result.Payload);

        LogErrors(logger, result.Errors);
        return Result<T>.Failure(result.Errors.ToArray());
    }

    public static Result HandleIdentityResult(this IdentityResult result, ILogger? logger = null)
    {
        if (result.Succeeded)
            return Result.Success();

        var errors = ConvertToErrorList(result.Errors);

        LogErrors(logger, errors, "Identity operation failed");
        return Result.Failure(errors.ToArray());
    }

    public static Result<T> HandleIdentityResult<T>(this IdentityResult result, T data, ILogger? logger = null)
    {
        if (result.Succeeded)
            return Result<T>.Success(data);

        var errors = ConvertToErrorList(result.Errors);

        LogErrors(logger, errors, "Identity operation failed");
        return Result<T>.Failure(errors.ToArray());
    }

    public static ActionResult ToActionResult(this Result result)
    {
        if (result.IsSucceeded)
            return new NoContentResult();
        
        return new ObjectResult(result.Errors[0]) {StatusCode = 400};
    }

    public static ActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSucceeded)
            return new OkObjectResult(result.Payload);

        return new ObjectResult(result.Errors[0]) { StatusCode = 400 };
    }

    private static void LogErrors(ILogger? logger, IReadOnlyList<Error> errors, string message = "Operation failed")
    {
        if (logger is null)
            return;

        logger.LogError(message);
        foreach (var error in errors)
            logger.LogWarning("ERROR: {code} | {description}", error.Code, error.Description);
    }

    private static Error MapIdentityError(IdentityError error)
    {
        return error.Code switch
        {
            "DuplicateUserName" => Error.User.UsernameAlreadyExists(),
            "InvalidUserName" => Error.Validation.InvalidInput($"Provided username is not valid {error.Description}"),
            "DuplicateEmail" => Error.User.EmailAlreadyExists(),
            "InvalidEmail" => Error.Validation.InvalidInput($"Provided email address is not valid {error.Description}"),
            "PasswordMismatch" => Error.Auth.InvalidCurrentPassword(),
            "PasswordTooShort" => Error.Auth.PasswordTooShort(),
            "PasswordRequiresDigit" => Error.Auth.PasswordRequiresDigit(),
            "PasswordRequiresUpper" => Error.Auth.PasswordRequiresUpper(),
            "PasswordRequiresNonAlphanumeric" => Error.Auth.PasswordRequiresNonAlphanumeric(),
            _ => new Error(error.Code, error.Description)
        };
    }

    private static IReadOnlyList<Error> ConvertToErrorList(IEnumerable<IdentityError> errors)
    {
        return errors.Select(MapIdentityError).ToList();
    }


}