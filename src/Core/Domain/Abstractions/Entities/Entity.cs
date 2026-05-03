namespace CleanArch.Domain.Abstractions.Entities;

/// <summary>
/// Base entity — provides identity and structural equality.
/// Cross-cutting concerns (auditing, soft-delete, domain events) are
/// expressed via composable interfaces: <see cref="IAuditableEntity"/>,
/// <see cref="ISoftDeletable"/>, <see cref="IHasDomainEvents"/>.
/// This keeps the base class thin and lets entities opt into only the
/// behaviors they need — following the Interface Segregation Principle.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    protected Entity(Guid id) => Id = id;

    protected Entity() { } // EF Core

    public Guid Id { get; private init; }

    // ─── Equality ───────────────────────────────────────
    public bool Equals(Entity? other)
    {
        return other is not null && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity entity && Equals(entity);
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? left, Entity? right)
    {
        return left is null && right is null ||
               left is not null && right is not null && left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}
