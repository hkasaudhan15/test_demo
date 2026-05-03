namespace CleanArch.Domain.Abstractions.Entities;

/// <summary>
/// Marks an entity as auditable — creation and modification timestamps
/// are automatically stamped by the infrastructure layer.
/// Implement on any entity that needs an audit trail.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedOnUtc { get; }
    string? CreatedBy { get; }
    DateTime? ModifiedOnUtc { get; }
    string? ModifiedBy { get; }
}
