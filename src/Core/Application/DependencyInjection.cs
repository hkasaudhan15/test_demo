using System.Reflection;
using CleanArch.Application.Abstractions.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArch.Application;

/// <summary>
/// Application-layer DI registration.
/// Call services.AddApplication() from Program.cs.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // ── MediatR + Pipeline Behaviors ────────────────
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        // Order matters: Logging → Idempotency → Validation → Performance
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        // ── FluentValidation ────────────────────────────
        services.AddValidatorsFromAssembly(assembly);

        // ── AutoMapper ──────────────────────────────────
        services.AddAutoMapper(assembly);

        return services;
    }
}
