namespace CleanArch.Domain.Abstractions.ValueObjects;

/// <summary>
/// Base class for Value Objects. Implements structural equality.
/// Used for domain concepts like Money, Address, DateRange, etc.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetAtomicValues();

    public bool Equals(ValueObject? other)
    {
        if (other is null || GetType() != other.GetType())
            return false;

        return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    }

    public override bool Equals(object? obj)
    {
        return obj is ValueObject valueObject && Equals(valueObject);
    }

    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Aggregate(default(int), (current, obj) =>
                HashCode.Combine(current, obj?.GetHashCode() ?? 0));
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !Equals(left, right);
    }
}
