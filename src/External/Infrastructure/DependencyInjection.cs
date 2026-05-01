using CleanArch.Application.Abstractions.Authentication;
using CleanArch.Application.Abstractions.Caching;
using CleanArch.Application.Abstractions.Clock;
using CleanArch.Application.Abstractions.Data;
using CleanArch.Application.Abstractions.Idempotency;
using CleanArch.CrossCutting.MultiTenancy;
using CleanArch.CrossCutting.MultiTenancy.Interceptors;
using CleanArch.CrossCutting.Outbox;
using CleanArch.CrossCutting.Outbox.Interceptors;
using CleanArch.Domain.Abstractions.Repositories;
using CleanArch.Infrastructure.Authentication.Jwt;
using CleanArch.Infrastructure.Caching.Redis;
using CleanArch.Infrastructure.Clock;
using CleanArch.Infrastructure.HealthChecks;
using CleanArch.Infrastructure.Idempotency;
using CleanArch.Infrastructure.Persistence.EFCore.Context;
using CleanArch.Infrastructure.Persistence.EFCore.Interceptors;
using CleanArch.Infrastructure.Persistence.EFCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArch.Infrastructure;

/// <summary>
/// Infrastructure-layer DI registration.
/// Call services.AddInfrastructure(configuration) from Program.cs.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Multi-Tenancy ─────────────────────────────────
        // Registers TenantProvider, HeaderTenantResolver, TenantInterceptor
        services.AddMultiTenancy();

        // ── EF Core Interceptors ─────────────────────────
        // Scoped interceptors depend on Scoped services
        // (ICurrentUserService, IPublisher, IDateTimeProvider)
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();

        // ── Outbox Pattern ───────────────────────────────
        // Registers OutboxInterceptor (singleton) + OutboxProcessor (hosted service)
        // + OutboxOptions from appsettings. When outbox is active, the
        // DomainEventDispatcherInterceptor is NOT registered — events are
        // published reliably via the outbox background processor instead.
        services.AddOutbox(configuration);

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                    sqlOptions.CommandTimeout(30);
                });

            options.AddInterceptors(
                sp.GetRequiredService<TenantInterceptor>(),
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<OutboxInterceptor>());
        });

        // Also register as DbContext so CrossCutting (OutboxProcessor) can resolve it
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // ── Unit of Work + Repositories ─────────────────
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

        // ── Idempotency ──────────────────────────────────
        services.AddScoped<IIdempotencyService, IdempotencyService>();

        // ── Dapper connection factory ───────────────────
        services.AddSingleton<IDbConnectionFactory>(_ =>
            new SqlConnectionFactory(configuration.GetConnectionString("DefaultConnection")!));

        // ── Authentication ──────────────────────────────
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddSingleton<ITokenService, JwtTokenService>();

        // ── Caching ─────────────────────────────────────
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });
        services.AddSingleton<ICacheService, RedisCacheService>();

        // ── Clock ───────────────────────────────────────
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        // ── Health Checks ───────────────────────────────
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database")
            .AddCheck<RedisHealthCheck>("redis");

        // ── Notifications (register your implementations) ──
        // services.AddScoped<IEmailService, SmtpEmailService>();
        // services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
