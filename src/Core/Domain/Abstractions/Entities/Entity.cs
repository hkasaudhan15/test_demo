using CleanArch.Domain.Abstractions.Events;

namespace CleanArch.Domain.Abstractions.Entities;

/// <summary>
/// Base entity with identity, audit fields, soft-delete, and domain event support.
/// Every entity in the system inherits from this.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected Entity(Guid id)
    {
        Id = id;
    }

    protected Entity() { } // EF Core

    public Guid Id { get; private init; }

    // ─── Audit Fields ───────────────────────────────────
    public DateTime CreatedOnUtc { get; internal set; }
    public string? CreatedBy { get; internal set; }
    public DateTime? ModifiedOnUtc { get; internal set; }
    public string? ModifiedBy { get; internal set; }

    // ─── Soft Delete ────────────────────────────────────
    public bool IsDeleted { get; internal set; }
    public DateTime? DeletedOnUtc { get; internal set; }
    public string? DeletedBy { get; internal set; }

    // ─── Domain Events ──────────────────────────────────
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

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
