using CleanArch.Infrastructure.Persistence.EFCore.Context;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CleanArch.Infrastructure.HealthChecks;

/// <summary>
/// Health check for database connectivity.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;

    public DatabaseHealthCheck(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.CanConnectAsync(cancellationToken);
            return HealthCheckResult.Healthy("Database is reachable and responsive.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Database is unreachable or not responding.",
                ex);
        }
    }
}
