using WorkoutTrackerApi.Services.Results;

namespace WorkoutTrackerApi.DTO.Global;

public class ApiResponse
{
    public bool IsSuccess { get;  }
    public string Message { get; }
    public Error? Error { get; }

    protected ApiResponse(bool isSuccess, string message, Error? error)
    {
        IsSuccess = isSuccess;
        Message = message;
        Error = error;
    }
    
    protected ApiResponse(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    public static ApiResponse Success(string message = "Success") => new(true, message, null);

    public static ApiResponse Failure(Error error, string message = "Error occurred")
    {
        if (error is null)
            throw new ArgumentException("Error is required for responses that has failed");

        return new ApiResponse(false, message, error);
    }

}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; }
    
    private ApiResponse(bool isSuccess, string message, Error? error) : base(isSuccess, message, error)
    {
        Data = default;
    }

    private ApiResponse(bool isSuccess, string message, Error? error, T data) : base(isSuccess, message, error)
    {
        Data = data;
    }

    public static ApiResponse<T> Success(string message, T data) => new(true, message, null, data);

}