using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore;
using WorkoutTrackerApi.DTO.Global;

namespace WorkoutTrackerApi.Filters;

public class ApiResponseTransformerFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        
        if (context.Result is not ObjectResult objectResult)
            return;

        if (objectResult.Value is not ApiResponse apiResponse)
            return;

        if (apiResponse.IsSuccess)
            return;
        
        var problemDetails = CreateProblemDetails(apiResponse, context.HttpContext);
        
        context.Result = new ObjectResult(problemDetails)
        {
            StatusCode = problemDetails.Status
        };

    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        
    }

    private int MapErrorCodeToStatusCode(string errorCode)
    {
        return errorCode switch
        {
            var code when code.Contains("Validation") => 400,
            var code when code.Contains("Auth") => 401,
            var code when code.Contains("Forbidden") => 403,
            var code when code.Contains("NotFound") => 404,
            var code when code.Contains("Conflict") => 409,
            _ => 500
        };
    }

    private string GetTitleFromStatusCode(int statusCode)
    {
        return statusCode switch
        {
            400 => "Validation",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            _ => "Server error occurred"
        };
    }

    private ProblemDetails CreateProblemDetails(ApiResponse apiResponse, HttpContext context)
    {
        var statusCode = MapErrorCodeToStatusCode(apiResponse.Error!.Code);
        var title = GetTitleFromStatusCode(statusCode);

        return new ProblemDetails()
        {
            Status = statusCode,
            Title = title,
            Detail = apiResponse.Message,
            Instance = context.Request.Path,
            Extensions =
            {
                ["errorCode"] = apiResponse.Error.Code
            }
        };

    }
}