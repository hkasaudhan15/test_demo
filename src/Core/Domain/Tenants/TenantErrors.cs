using CleanArch.Domain.Primitives.Results;

namespace CleanArch.Domain.Tenants;

public static class TenantErrors
{
    public static Error NotFound(Guid id) =>
        new("Tenant.NotFound",
            $"Tenant with ID '{id}' was not found.",
            ErrorType.NotFound);

    public static Error NotFoundByIdentifier(string identifier) =>
        new("Tenant.NotFound",
            $"Tenant '{identifier}' was not found.",
            ErrorType.NotFound);

    public static Error DuplicateIdentifier(string identifier) =>
        new("Tenant.DuplicateIdentifier",
            $"A tenant with identifier '{identifier}' already exists.",
            ErrorType.Conflict);

    public static readonly Error AlreadyActive =
        new("Tenant.AlreadyActive",
            "Tenant is already active.",
            ErrorType.Conflict);

    public static readonly Error NotActive =
        new("Tenant.NotActive",
            "Tenant is not currently active.",
            ErrorType.Conflict);

    public static readonly Error Suspended =
        new("Tenant.Suspended",
            "Tenant is suspended and cannot perform this operation.",
            ErrorType.Forbidden);

    public static readonly Error Deactivated =
        new("Tenant.Deactivated",
            "Tenant has been deactivated.",
            ErrorType.Forbidden);

    public static readonly Error SubscriptionExpired =
        new("Tenant.SubscriptionExpired",
            "Tenant subscription has expired.",
            ErrorType.Forbidden);
}
