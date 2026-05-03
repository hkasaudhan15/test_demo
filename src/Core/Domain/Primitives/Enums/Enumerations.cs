namespace CleanArch.Domain.Primitives.Enums;

/// <summary>
/// Common status enum — extend or create domain-specific enums as needed.
/// </summary>
public enum Status
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    Archived = 4
}

/// <summary>
/// Smart enum base — for enums that carry behavior.
/// </summary>
public abstract class Enumeration<TEnum> : IEquatable<Enumeration<TEnum>>
    where TEnum : Enumeration<TEnum>
{
    private static readonly Lazy<Dictionary<int, TEnum>> _enumerations = new(CreateEnumerations);

    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; }
    public string Name { get; }

    public static TEnum? FromId(int id)
    {
        return _enumerations.Value.GetValueOrDefault(id);
    }

    public static TEnum? FromName(string name)
    {
        return _enumerations.Value.Values.SingleOrDefault(e => e.Name == name);
    }

    public static IReadOnlyCollection<TEnum> GetAll()
    {
        return _enumerations.Value.Values.ToList().AsReadOnly();
    }

    public bool Equals(Enumeration<TEnum>? other)
    {
        return other is not null && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Enumeration<TEnum> other && Equals(other);
    }

    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => Name;

    private static Dictionary<int, TEnum> CreateEnumerations()
    {
        return typeof(TEnum)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(TEnum))
            .Select(f => (TEnum)f.GetValue(null)!)
            .ToDictionary(e => e.Id);
    }
}
