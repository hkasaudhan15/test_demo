using System.Text.RegularExpressions;

namespace CleanArch.Domain.Abstractions.ValueObjects;

/// <summary>
/// Value object representing a validated email address.
/// Validates format on creation — invalid emails cannot exist in the domain.
///
/// Usage:
///   var email = Email.Create("user@example.com");
///   entity.ChangeEmail(email); // type-safe, always valid
/// </summary>
public sealed partial class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email address is required.", nameof(email));

        var normalized = email.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(normalized))
            throw new ArgumentException($"Invalid email format: '{email}'.", nameof(email));

        if (normalized.Length > 256)
            throw new ArgumentException("Email address cannot exceed 256 characters.", nameof(email));

        return new Email(normalized);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
