using FluentValidation;

namespace CleanArch.Application.Common.Validators;

/// <summary>
/// Reusable validation rules for common scenarios.
/// </summary>
public static class CommonValidators
{
    /// <summary>
    /// Validates email address format and length.
    /// </summary>
    public static IRuleBuilder<T, string> MustBeValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256)
            .WithMessage("Email must be a valid email address and not exceed 256 characters.");
    }

    /// <summary>
    /// Validates strong password requirements.
    /// </summary>
    public static IRuleBuilder<T, string> MustBeStrongPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character.");
    }

    /// <summary>
    /// Validates that string doesn't contain potentially unsafe characters.
    /// </summary>
    public static IRuleBuilder<T, string> MustBeSafeString<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(value => !value.Contains('<') && !value.Contains('>'))
            .WithMessage("Input contains potentially unsafe characters (< or >).");
    }

    /// <summary>
    /// Validates phone number format (basic international format).
    /// </summary>
    public static IRuleBuilder<T, string> MustBeValidPhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Phone number must be in valid international format (E.164).");
    }

    /// <summary>
    /// Validates URL format.
    /// </summary>
    public static IRuleBuilder<T, string> MustBeValidUrl<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && 
                        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Must be a valid HTTP or HTTPS URL.");
    }

    /// <summary>
    /// Validates that Guid is not empty.
    /// </summary>
    public static IRuleBuilder<T, Guid> MustNotBeEmpty<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .Must(guid => guid != Guid.Empty)
            .WithMessage("ID must not be empty.");
    }

    /// <summary>
    /// Validates that collection is not empty.
    /// </summary>
    public static IRuleBuilder<T, IEnumerable<TElement>> MustNotBeEmpty<T, TElement>(
        this IRuleBuilder<T, IEnumerable<TElement>> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .Must(collection => collection.Any())
            .WithMessage("Collection must contain at least one item.");
    }

    /// <summary>
    /// Validates that value is within a specific range.
    /// </summary>
    public static IRuleBuilder<T, int> MustBeInRange<T>(
        this IRuleBuilder<T, int> ruleBuilder, 
        int min, 
        int max)
    {
        return ruleBuilder
            .InclusiveBetween(min, max)
            .WithMessage($"Value must be between {min} and {max}.");
    }

    /// <summary>
    /// Validates that decimal is positive.
    /// </summary>
    public static IRuleBuilder<T, decimal> MustBePositive<T>(this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithMessage("Value must be greater than zero.");
    }

    /// <summary>
    /// Validates that date is not in the past.
    /// </summary>
    public static IRuleBuilder<T, DateTime> MustNotBeInPast<T>(this IRuleBuilder<T, DateTime> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Date must not be in the past.");
    }

    /// <summary>
    /// Validates that date is not in the future.
    /// </summary>
    public static IRuleBuilder<T, DateTime> MustNotBeInFuture<T>(this IRuleBuilder<T, DateTime> ruleBuilder)
    {
        return ruleBuilder
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Date must not be in the future.");
    }
}
