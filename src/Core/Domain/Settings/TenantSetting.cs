using CleanArch.Domain.Abstractions.Entities;

namespace CleanArch.Domain.Settings;

/// <summary>
/// Per-tenant setting override — extends the global settings system.
///
/// Resolution strategy (cascading override):
///   1. Check TenantSettings for (TenantId, Key)
///   2. If not found → fall back to GlobalSettings for (Key)
///   3. If not found → throw KeyNotFoundException
///
/// This allows tenants to customize their configuration while
/// inheriting global defaults for unoverridden keys.
///
/// Features:
///   - Same key/group/datatype structure as GlobalSetting
///   - TenantId scoping for data isolation
///   - Auditable (who changed what, when)
///   - IsOverridden computed property for admin UI display
///   - Bulk import/export support via group operations
/// </summary>
public sealed class TenantSetting : Entity, IAuditableEntity
{
    public string TenantId { get; private set; } = string.Empty;
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string Group { get; private set; } = string.Empty;
    public SettingDataType DataType { get; private set; }
    public string? Description { get; private set; }

    // ─── IAuditableEntity ────────────────────────────────
    public DateTime CreatedOnUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }
    public string? ModifiedBy { get; set; }

    private TenantSetting() { } // EF Core

    private TenantSetting(
        Guid id,
        string tenantId,
        string key,
        string value,
        string group,
        SettingDataType dataType,
        string? description) : base(id)
    {
        TenantId = tenantId;
        Key = key;
        Value = value;
        Group = group;
        DataType = dataType;
        Description = description;
    }

    public static TenantSetting Create(
        string tenantId,
        string key,
        string value,
        string group,
        SettingDataType dataType = SettingDataType.Text,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID is required.", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Setting key is required.", nameof(key));

        if (string.IsNullOrWhiteSpace(group))
            throw new ArgumentException("Setting group is required.", nameof(group));

        ValidateValue(value, dataType);

        return new TenantSetting(
            Guid.NewGuid(),
            tenantId.Trim(),
            key.Trim(),
            value,
            group.Trim(),
            dataType,
            description?.Trim());
    }

    public void UpdateValue(string newValue)
    {
        ValidateValue(newValue, DataType);
        Value = newValue;
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
    }

    // ─── Typed Accessors ─────────────────────────────────

    public int GetAsInt() => int.Parse(Value);
    public decimal GetAsDecimal() => decimal.Parse(Value);
    public bool GetAsBool() => bool.Parse(Value);
    public DateTime GetAsDateTime() => DateTime.Parse(Value);

    // ─── Validation ──────────────────────────────────────

    private static void ValidateValue(string value, SettingDataType dataType)
    {
        var isValid = dataType switch
        {
            SettingDataType.WholeNumber => int.TryParse(value, out _),
            SettingDataType.Number => decimal.TryParse(value, out _),
            SettingDataType.Flag => bool.TryParse(value, out _),
            SettingDataType.Timestamp => DateTime.TryParse(value, out _),
            SettingDataType.Text => true,
            SettingDataType.Json => true,
            _ => true
        };

        if (!isValid)
            throw new ArgumentException(
                $"Value '{value}' is not valid for data type '{dataType}'.",
                nameof(value));
    }
}
