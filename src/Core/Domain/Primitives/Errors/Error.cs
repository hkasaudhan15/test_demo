namespace CleanArch.Domain.Primitives.Results;

/// <summary>
/// Categorizes errors for HTTP status code mapping.
/// Used by BaseApiController to avoid fragile string matching.
/// </summary>
public enum ErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Unauthorized = 3,
    Forbidden = 4,
    Conflict = 5,
    BadRequest = 6,
    Internal = 7
}

/// <summary>
/// Strongly-typed domain error — used with the Result pattern.
/// Each bounded context defines its own static error catalog.
/// </summary>
public record Error(string Code, string Message, ErrorType Type = ErrorType.BadRequest)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.", ErrorType.BadRequest);

    public static implicit operator Result(Error error) => Result.Failure(error);
}

/// <summary>
/// Validation error carrying multiple field-level errors.
/// </summary>
public sealed record ValidationError : Error
{
    public ValidationError(Error[] errors)
        : base("Validation.General", "One or more validation errors occurred.", ErrorType.Validation)
    {
        Errors = errors;
    }

    public Error[] Errors { get; }
}

