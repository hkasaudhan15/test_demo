using CleanArch.Domain.Primitives.Results;

namespace CleanArch.Domain.Settings;

public static class SettingErrors
{
    public static Error NotFound(string key) =>
        new("Setting.NotFound", $"Setting with key '{key}' was not found.", ErrorType.NotFound);

    public static Error DuplicateKey(string key) =>
        new("Setting.DuplicateKey", $"A setting with key '{key}' already exists.", ErrorType.Conflict);

    public static readonly Error ReadOnly =
        new("Setting.ReadOnly", "This setting is read-only and cannot be modified.", ErrorType.Forbidden);
}
