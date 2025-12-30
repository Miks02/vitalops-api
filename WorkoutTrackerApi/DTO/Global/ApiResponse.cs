using WorkoutTrackerApi.Services.Results;

namespace WorkoutTrackerApi.DTO.Global;

public class ApiResponse
{
    public bool IsSuccess { get;  }
    public string Message { get; }
    public IReadOnlyList<Error> Errors { get; }

    protected ApiResponse(bool isSuccess, string message, IReadOnlyList<Error> errors)
    {
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors;
    }

    public static ApiResponse Success(string message) => new(true, message, []);

    public static ApiResponse Failure(string message, params Error[] errors)
    {
        if (errors.Length == 0)
            throw new ArgumentException("At least one error is required for failure response");

        return new ApiResponse(false, message, errors);
    }

}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }
    
    private ApiResponse(bool isSuccess, string message, IReadOnlyList<Error> errors) : base(isSuccess, message, errors)
    {
        Data = default;
    }

    private ApiResponse(bool isSuccess, string message, IReadOnlyList<Error> errors, T data) : base(isSuccess, message, errors)
    {
        Data = data;
    }

    public static ApiResponse<T> Success(string message, T data) => new(true, message, [], data);
    
    public new static ApiResponse Failure(string message, params Error[] errors)
    {
        if (errors.Length == 0)
            throw new ArgumentException("At least one error is required for failure response");

        return new ApiResponse<T>(false, message, errors);
    }

}