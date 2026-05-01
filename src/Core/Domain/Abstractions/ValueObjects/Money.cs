namespace CleanArch.Domain.Abstractions.ValueObjects;

/// <summary>
/// Value object representing a monetary amount with currency.
/// Prevents mixing currencies in arithmetic operations.
///
/// Usage:
///   var price = Money.Create(19.99m, "USD");
///   var total = price + Money.Create(5.00m, "USD"); // OK
///   var bad = price + Money.Create(5.00m, "EUR");   // throws
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency code is required.", nameof(currency));

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO 4217 code.", nameof(currency));

        return new Money(amount, currency.ToUpperInvariant());
    }

    public static Money Zero(string currency) => Create(0, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return Create(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return Create(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor) => Create(Amount * factor, Currency);

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:F2} {Currency}";

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot perform operation on different currencies: {Currency} vs {other.Currency}.");
    }
}
