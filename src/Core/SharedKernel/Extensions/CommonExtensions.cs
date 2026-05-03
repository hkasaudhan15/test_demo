namespace CleanArch.SharedKernel.Extensions;

/// <summary>
/// Common string extension methods.
/// </summary>
public static class StringExtensions
{
    public static string ToSnakeCase(this string input)
    {
        return string.Concat(input.Select((ch, i) =>
            i > 0 && char.IsUpper(ch) ? "_" + ch : ch.ToString())).ToLowerInvariant();
    }

    public static string ToCamelCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToLowerInvariant(input[0]) + input[1..];
    }

    public static bool IsNullOrEmpty(this string? value) => string.IsNullOrWhiteSpace(value);
    public static bool HasValue(this string? value) => !string.IsNullOrWhiteSpace(value);

    public static string Truncate(this string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength] + "...";
    }
}

/// <summary>
/// Common DateTime extension methods.
/// </summary>
public static class DateTimeExtensions
{
    public static bool IsInPast(this DateTime dateTime) => dateTime < DateTime.UtcNow;
    public static bool IsInFuture(this DateTime dateTime) => dateTime > DateTime.UtcNow;

    public static DateTime StartOfDay(this DateTime dateTime) => dateTime.Date;
    public static DateTime EndOfDay(this DateTime dateTime) => dateTime.Date.AddDays(1).AddTicks(-1);
}

/// <summary>
/// Collection extension methods.
/// </summary>
public static class CollectionExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection is null || !collection.Any();
    }

    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, bool condition, Func<T, bool> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }
}
