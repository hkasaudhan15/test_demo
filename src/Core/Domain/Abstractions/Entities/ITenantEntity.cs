namespace CleanArch.Domain.Abstractions.Entities;

/// <summary>
/// Marker interface for entities that belong to a specific tenant.
/// Entities implementing this interface will:
///   1. Have TenantId automatically stamped on insert (via TenantInterceptor)
///   2. Be filtered by TenantId in all queries (via global query filter)
///
/// Usage:
///   public class Order : AggregateRoot, ITenantEntity
///   {
///       public string TenantId { get; set; } = string.Empty;
///   }
/// </summary>
public interface ITenantEntity
{
    string TenantId { get; set; }
}
