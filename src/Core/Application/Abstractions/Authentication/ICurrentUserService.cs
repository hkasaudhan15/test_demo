namespace CleanArch.Application.Abstractions.Authentication;

/// <summary>
/// Abstracts the current authenticated user context.
/// Implemented in Presentation layer using HttpContext.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    string? TenantId { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
    bool IsInRole(string role);
}
