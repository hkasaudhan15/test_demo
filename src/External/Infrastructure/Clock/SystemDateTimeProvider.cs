using CleanArch.Application.Abstractions.Clock;

namespace CleanArch.Infrastructure.Clock;

/// <summary>
/// System clock implementation — returns real UTC time.
/// Swap with FakeDateTimeProvider in tests.
/// </summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
