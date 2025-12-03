namespace WorkoutTrackerApi.Services.Results;

public class ServiceResult
{
    public bool IsSucceeded { get; }

    public IReadOnlyList<Error> Errors { get; }

    protected ServiceResult(bool isSucceeded, IReadOnlyList<Error> errors)
    {
        IsSucceeded = isSucceeded;
        Errors = errors;
    }

    public static ServiceResult Success() => new(true, []);

    public static ServiceResult Failure(params Error[] errors)
    {
        if (errors.Length == 0)
            throw new ArgumentException("At least one error must be provided within a failure");

        return new ServiceResult(false, errors.AsReadOnly());

    }

}

public class ServiceResult<T> : ServiceResult
{
    private T? Payload { get; }

    private ServiceResult(bool isSucceeded, IReadOnlyList<Error> errors) : base(isSucceeded, errors)
    {
        Payload = default;
    }
    
    private ServiceResult(bool isSucceeded, IReadOnlyList<Error> errors, T? payload) : base(isSucceeded, errors)
    {
        Payload = payload;
    }

    public new static ServiceResult<T> Success() => new(true, []);

    public static ServiceResult<T> Success(T payload)
    {
        if (payload is null)
            throw new ArgumentNullException(nameof(payload), "Payload cannot be null");

        return new ServiceResult<T>(true, [], payload);
    }

    public new static ServiceResult<T> Failure(params Error[] errors)
    {
        if(errors.Length == 0)
            throw new ArgumentException("At least one error must be provided within a failure");

        return new ServiceResult<T>(false, errors.AsReadOnly());
    }

}