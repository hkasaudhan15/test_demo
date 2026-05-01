using CleanArch.CrossCutting.Outbox.Abstractions;
using CleanArch.CrossCutting.Outbox.Interceptors;
using CleanArch.CrossCutting.Outbox.Processing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArch.CrossCutting.Outbox;

/// <summary>
/// Registers outbox pattern services:
///   - <see cref="OutboxInterceptor"/> (EF Core interceptor, must be added to DbContext options)
///   - <see cref="OutboxProcessor"/> (hosted background service)
///   - <see cref="OutboxOptions"/> (bound from appsettings.json)
/// </summary>
public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddOutbox(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OutboxOptions>(
            configuration.GetSection(OutboxOptions.SectionName));

        services.AddSingleton<OutboxInterceptor>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
