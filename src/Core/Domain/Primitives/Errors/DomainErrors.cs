using CleanArch.Domain.Primitives.Results;

namespace CleanArch.Domain.Primitives.Errors;

/// <summary>
/// Static error catalog — define all domain errors here.
/// Organized by bounded context / feature area.
/// Add your domain-specific errors as static fields.
/// </summary>
public static class DomainErrors
{
    // ─── Example: General ───────────────────────────────
    public static class General
    {
        public static readonly Error NotFound = new("General.NotFound", "The requested resource was not found.", ErrorType.NotFound);
        public static readonly Error AlreadyExists = new("General.AlreadyExists", "The resource already exists.", ErrorType.Conflict);
        public static readonly Error Unauthorized = new("General.Unauthorized", "You are not authorized to perform this action.", ErrorType.Unauthorized);
        public static readonly Error Forbidden = new("General.Forbidden", "Access to this resource is forbidden.", ErrorType.Forbidden);
        public static readonly Error Conflict = new("General.Conflict", "A conflict occurred with the current state of the resource.", ErrorType.Conflict);
        public static readonly Error BadRequest = new("General.BadRequest", "The request was invalid.", ErrorType.BadRequest);
    }

    // ─── Example: Authentication ────────────────────────
    // Add your bounded context errors here when creating features:
    //
    // public static class Authentication
    // {
    //     public static readonly Error InvalidCredentials = new("Auth.InvalidCredentials", "Invalid email or password.", ErrorType.Unauthorized);
    //     public static readonly Error EmailNotVerified = new("Auth.EmailNotVerified", "Email address has not been verified.", ErrorType.BadRequest);
    //     public static readonly Error AccountLocked = new("Auth.AccountLocked", "Account has been locked.", ErrorType.Forbidden);
    //     public static readonly Error TokenExpired = new("Auth.TokenExpired", "The token has expired.", ErrorType.Unauthorized);
    // }
}

