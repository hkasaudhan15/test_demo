using System.Runtime.CompilerServices;

namespace CleanArch.Domain.Primitives.Guards;

/// <summary>
/// Guard clauses for domain invariant enforcement.
/// Use in entity constructors and factory methods.
/// </summary>
public static class Guard
{
    public static string AgainstNullOrEmpty(
        string? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"'{paramName}' cannot be null or empty.", paramName);

        return value;
    }

    public static T AgainstNull<T>(
        T? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(paramName);

        return value;
    }

    public static Guid AgainstEmptyGuid(
        Guid value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value == Guid.Empty)
            throw new ArgumentException($"'{paramName}' cannot be empty.", paramName);

        return value;
    }

    public static int AgainstNegative(
        int value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, $"'{paramName}' cannot be negative.");

        return value;
    }

    public static decimal AgainstNegative(
        decimal value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, $"'{paramName}' cannot be negative.");

        return value;
    }

    public static string AgainstOverflow(
        string value,
        int maxLength,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value.Length > maxLength)
            throw new ArgumentException($"'{paramName}' cannot exceed {maxLength} characters.", paramName);

        return value;
    }

    public static T AgainstInvalidEnum<T>(
        T value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new ArgumentException($"'{paramName}' contains an invalid enum value: {value}.", paramName);

        return value;
    }
}
