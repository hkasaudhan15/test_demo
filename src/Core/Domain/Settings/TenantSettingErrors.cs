using CleanArch.Domain.Primitives.Results;

namespace CleanArch.Domain.Settings;

public static class TenantSettingErrors
{
    public static Error NotFound(string tenantId, string key) =>
        new("TenantSetting.NotFound",
            $"Tenant setting with key '{key}' not found for tenant '{tenantId}'.",
            ErrorType.NotFound);

    public static readonly Error TenantRequired =
        new("TenantSetting.TenantRequired",
            "A valid tenant context is required to manage tenant settings.",
            ErrorType.BadRequest);

    public static Error GlobalSettingNotFound(string key) =>
        new("TenantSetting.GlobalNotFound",
            $"Cannot override setting '{key}' — no matching global setting exists.",
            ErrorType.NotFound);

    public static Error DataTypeMismatch(string key, SettingDataType expected, SettingDataType actual) =>
        new("TenantSetting.DataTypeMismatch",
            $"Setting '{key}' expects data type '{expected}' but received '{actual}'.",
            ErrorType.Validation);
}
