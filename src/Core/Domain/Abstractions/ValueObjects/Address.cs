namespace CleanArch.Domain.Abstractions.ValueObjects;

/// <summary>
/// Value object representing a postal address.
/// Configured as an EF Core owned type or complex type for persistence.
///
/// Usage in entity:
///   public Address ShippingAddress { get; private set; }
///
/// EF Core config:
///   builder.OwnsOne(e => e.ShippingAddress);
/// </summary>
public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string Country { get; }

    private Address(string street, string city, string state, string zipCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
    }

    public static Address Create(string street, string city, string state, string zipCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required.", nameof(street));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required.", nameof(city));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country is required.", nameof(country));

        return new Address(street.Trim(), city.Trim(), state.Trim(), zipCode.Trim(), country.Trim());
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Country;
    }

    public override string ToString() => $"{Street}, {City}, {State} {ZipCode}, {Country}";
}
