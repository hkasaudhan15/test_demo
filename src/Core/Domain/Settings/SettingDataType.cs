namespace CleanArch.Domain.Settings;

/// <summary>
/// Data types supported by the settings system.
/// The value is always stored as string; this enum controls
/// serialization, validation, and UI rendering.
/// </summary>
public enum SettingDataType
{
    Text = 0,
    WholeNumber = 1,
    Number = 2,
    Flag = 3,
    Json = 4,
    Timestamp = 5
}
