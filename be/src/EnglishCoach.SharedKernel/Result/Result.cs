namespace EnglishCoach.SharedKernel.Result;

public struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    public bool IsSuccess => _error is null;
    public bool IsFailure => _error is not null;
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot get value from failed result");
    public Error Error => _error ?? throw new InvalidOperationException("Cannot get error from successful result");

    private Result(T? value, Error? error)
    {
        _value = value;
        _error = error;
    }

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(Error error) => new(default, error);

    public Result<TOut> Map<TOut>(Func<T, TOut> mapper) =>
        IsSuccess ? Result<TOut>.Success(mapper(_value!)) : Result<TOut>.Failure(_error!);

    public T UnwrapOr(T defaultValue) => IsSuccess ? _value! : defaultValue;

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}

public struct Result
{
    private readonly Error? _error;

    public bool IsSuccess => _error is null;
    public bool IsFailure => _error is not null;
    public Error Error => _error ?? throw new InvalidOperationException("Cannot get error from successful result");

    private Result(Error? error) => _error = error;

    public static Result Success() => new(null);
    public static Result Failure(Error error) => new(error);

    public static implicit operator Result(Error error) => Failure(error);
}

public record Error(string Code, string Message, string? Details = null)
{
    public static Error NotFound(string entity, string id) =>
        new("NOT_FOUND", $"{entity} with id '{id}' was not found.");

    public static Error Validation(string message) =>
        new("VALIDATION_ERROR", message);

    public static Error Forbidden(string message = "Operation not allowed") =>
        new("FORBIDDEN", message);

    public static Error Conflict(string message) =>
        new("CONFLICT", message);

    public static Error Internal(string message = "An unexpected error occurred") =>
        new("INTERNAL_ERROR", message);

    public static Error Domain(string message) =>
        new("DOMAIN_ERROR", message);
}