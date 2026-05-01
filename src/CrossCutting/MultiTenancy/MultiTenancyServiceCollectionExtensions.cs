using CleanArch.CrossCutting.MultiTenancy.Abstractions;
using CleanArch.CrossCutting.MultiTenancy.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArch.CrossCutting.MultiTenancy;

/// <summary>
/// DI registration for multi-tenancy services.
/// Call services.AddMultiTenancy() from Infrastructure DI.
///
/// Registers:
///   - TenantProvider (scoped) as both concrete + ITenantProvider
///   - HeaderTenantResolver as ITenantResolver (swap for subdomain/claim resolver)
///   - TenantInterceptor (scoped) — must be added to DbContext options
/// </summary>
public static class MultiTenancyServiceCollectionExtensions
{
    public static IServiceCollection AddMultiTenancy(this IServiceCollection services)
    {
        // Scoped: one tenant per request
        services.AddScoped<TenantProvider>();
        services.AddScoped<ITenantProvider>(sp => sp.GetRequiredService<TenantProvider>());

        // Resolver strategy — swap implementation here for subdomain/claim-based
        services.AddScoped<ITenantResolver, HeaderTenantResolver>();

        // EF Core interceptor
        services.AddScoped<TenantInterceptor>();

        return services;
    }
}
