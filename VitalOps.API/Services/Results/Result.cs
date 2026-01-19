using Microsoft.AspNetCore.Identity;

namespace VitalOps.API.Services.Results;

public class Result
{
    public bool IsSucceeded { get; }

    public IReadOnlyList<Error> Errors { get; }

    protected Result(bool isSucceeded, IReadOnlyList<Error> errors)
    {
        IsSucceeded = isSucceeded;
        Errors = errors;
    }

    public static Result Success() => new(true, []);

    public static Result Failure(params Error[] errors)
    {
        if (errors.Length == 0)
            throw new ArgumentException("At least one error must be provided within a failure");

        return new Result(false, errors.AsReadOnly());

    }

}

public class Result<T> : Result
{
    public T? Payload { get; }

    private Result(bool isSucceeded, IReadOnlyList<Error> errors) : base(isSucceeded, errors)
    {
        Payload = default;
    }
    
    private Result(bool isSucceeded, IReadOnlyList<Error> errors, T? payload) : base(isSucceeded, errors)
    {
        Payload = payload;
    }

    public new static Result<T> Success() => new(true, []);

    public static Result<T> Success(T payload)
    {
        if (payload is null)
            throw new ArgumentNullException(nameof(payload), "Payload cannot be null");

        return new Result<T>(true, [], payload);
    }

    public new static Result<T> Failure(params Error[] errors)
    {
        if(errors.Length == 0)
            throw new ArgumentException("At least one error must be provided within a failure");



        return new Result<T>(false, errors.AsReadOnly());
    }

    

}