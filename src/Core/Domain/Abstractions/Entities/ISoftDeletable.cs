namespace CleanArch.Domain.Abstractions.Entities;

/// <summary>
/// Marks an entity as soft-deletable. Instead of physical DELETE,
/// the entity is flagged as deleted and filtered out by a global query filter.
/// Infrastructure layer intercepts Delete operations and converts them.
/// Not every entity needs this — only implement where business requires it.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedOnUtc { get; }
    string? DeletedBy { get; }
}
