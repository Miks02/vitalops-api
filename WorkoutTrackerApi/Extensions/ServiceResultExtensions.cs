using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerApi.DTO.Global;
using WorkoutTrackerApi.Services.Results;

namespace WorkoutTrackerApi.Extensions;

public static class ServiceResultExtensions
{
    public static IActionResult ToActionResult(this ServiceResult result, string successMessage = "Success")
    {
        if (result.IsSucceeded)
        {
            return new OkObjectResult(ApiResponse.Success(successMessage));
        }

        return new ObjectResult(ApiResponse.Failure(result.Errors.First()));
    }
    
    public static IActionResult ToActionResult<T>(this ServiceResult<T> result, string successMessage = "Success")
    {
        if (result.IsSucceeded)
        {
            return new OkObjectResult(ApiResponse<T>.Success(successMessage, result.Payload!));

        }

        return new ObjectResult(ApiResponse.Failure(result.Errors.First()));
    }
}