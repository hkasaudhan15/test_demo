using System.Security.Claims;
using CleanArch.Application.Abstractions.Authentication;

namespace CleanArch.Presentation.Api.Extensions;

/// <summary>
/// Extracts current user info from HttpContext claims.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
    public string? TenantId => _httpContextAccessor.HttpContext?.Request?.Headers["X-Tenant-Id"].FirstOrDefault();
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public IEnumerable<string> Roles =>
        _httpContextAccessor.HttpContext?.User?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value) ?? Enumerable.Empty<string>();

    public bool IsInRole(string role) => Roles.Contains(role);
}
