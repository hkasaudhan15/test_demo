using CleanArch.Domain.Abstractions.Entities;

namespace CleanArch.Domain.Settings;

/// <summary>
/// Database-backed application setting — key-value with metadata.
///
/// Features:
///   - Grouped by category (e.g., "Email", "Security", "UI")
///   - Typed values (string/int/decimal/bool/json/datetime)
///   - Optional description for admin UI
///   - IsReadOnly flag for system settings that shouldn't be edited at runtime
///   - Display order for admin panel rendering
///   - Inherits IAuditableEntity (audit trail of who changed what, when)
///
/// The Value is always stored as string; consumers use the typed accessors
/// (GetAsInt, GetAsBool, etc.) or the ISettingsService for type-safe reads.
/// </summary>
public sealed class GlobalSetting : Entity, IAuditableEntity
{
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string Group { get; private set; } = string.Empty;
    public SettingDataType DataType { get; private set; }
    public string? Description { get; private set; }
    public bool IsReadOnly { get; private set; }
    public int DisplayOrder { get; private set; }

    // ─── IAuditableEntity ────────────────────────────────
    public DateTime CreatedOnUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }
    public string? ModifiedBy { get; set; }

    private GlobalSetting() { } // EF Core

    private GlobalSetting(
        Guid id,
        string key,
        string value,
        string group,
        SettingDataType dataType,
        string? description,
        bool isReadOnly,
        int displayOrder) : base(id)
    {
        Key = key;
        Value = value;
        Group = group;
        DataType = dataType;
        Description = description;
        IsReadOnly = isReadOnly;
        DisplayOrder = displayOrder;
    }

    public static GlobalSetting Create(
        string key,
        string value,
        string group,
        SettingDataType dataType = SettingDataType.Text,
        string? description = null,
        bool isReadOnly = false,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Setting key is required.", nameof(key));

        if (string.IsNullOrWhiteSpace(group))
            throw new ArgumentException("Setting group is required.", nameof(group));

        ValidateValue(value, dataType);

        return new GlobalSetting(
            Guid.NewGuid(),
            key.Trim(),
            value,
            group.Trim(),
            dataType,
            description?.Trim(),
            isReadOnly,
            displayOrder);
    }

    public void UpdateValue(string newValue)
    {
        if (IsReadOnly)
            throw new InvalidOperationException($"Setting '{Key}' is read-only and cannot be modified.");

        ValidateValue(newValue, DataType);
        Value = newValue;
    }

    public void UpdateMetadata(string? description, int displayOrder)
    {
        Description = description?.Trim();
        DisplayOrder = displayOrder;
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
            SettingDataType.Json => true, // JSON validation left to consumer
            _ => true
        };

        if (!isValid)
            throw new ArgumentException(
                $"Value '{value}' is not valid for data type '{dataType}'.",
                nameof(value));
    }
}
