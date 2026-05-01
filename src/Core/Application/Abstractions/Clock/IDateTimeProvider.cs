namespace CleanArch.Application.Abstractions.Clock;

/// <summary>
/// Clock abstraction — makes time testable.
/// Never use DateTime.UtcNow directly; inject this instead.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
