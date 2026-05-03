namespace CleanArch.Domain.Primitives.Exceptions;

/// <summary>
/// Base domain exception — only thrown for truly exceptional scenarios.
/// Prefer Result pattern for expected failures.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when a business invariant is violated that cannot be expressed via Result.
/// </summary>
public sealed class BusinessRuleException : DomainException
{
    public string Code { get; }

    public BusinessRuleException(string code, string message) : base(message)
    {
        Code = code;
    }
}

/// <summary>
/// Thrown when a concurrent modification is detected (optimistic concurrency).
/// </summary>
public sealed class ConcurrencyException : DomainException
{
    public ConcurrencyException(string message) : base(message) { }
}
