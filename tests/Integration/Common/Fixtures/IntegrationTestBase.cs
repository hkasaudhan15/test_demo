using CleanArch.Infrastructure.Persistence.EFCore.Context;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CleanArch.IntegrationTests.Common.Fixtures;

/// <summary>
/// Base class for integration tests with in-memory database.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    protected HttpClient Client { get; }
    protected IServiceScope Scope { get; }
    protected ApplicationDbContext DbContext { get; }

    protected IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        var appFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                });

                // Remove background services for testing
                var hostedServices = services
                    .Where(d => d.ServiceType.Name.Contains("HostedService"))
                    .ToList();

                foreach (var service in hostedServices)
                {
                    services.Remove(service);
                }
            });
        });

        Client = appFactory.CreateClient();
        Scope = appFactory.Services.CreateScope();
        DbContext = Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    public async Task InitializeAsync()
    {
        await DbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        Scope.Dispose();
        Client.Dispose();
    }

    /// <summary>
    /// Seeds test data into the database.
    /// </summary>
    protected async Task SeedAsync<TEntity>(params TEntity[] entities) where TEntity : class
    {
        DbContext.Set<TEntity>().AddRange(entities);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Clears all data from a specific entity set.
    /// </summary>
    protected async Task ClearAsync<TEntity>() where TEntity : class
    {
        DbContext.Set<TEntity>().RemoveRange(DbContext.Set<TEntity>());
        await DbContext.SaveChangesAsync();
    }
}
