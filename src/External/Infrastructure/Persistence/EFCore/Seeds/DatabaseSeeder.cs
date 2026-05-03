using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanArch.Infrastructure.Persistence.EFCore.Seeds;

/// <summary>
/// Database seeder — called on startup.
/// Auto-migrates in development, seeds reference data.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Context.ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Context.ApplicationDbContext>>();

        try
        {
            // Auto-migrate in development
            var env = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            if (env.EnvironmentName == "Development")
            {
                logger.LogInformation("Applying database migrations...");
                await context.Database.MigrateAsync();
            }

            // Seed reference data
            await SeedReferenceDataAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task SeedReferenceDataAsync(
        Context.ApplicationDbContext context,
        ILogger logger)
    {
        // Add your seed data here:
        //
        // if (!await context.Roles.AnyAsync())
        // {
        //     context.Roles.AddRange(
        //         new Role("Admin"),
        //         new Role("User"),
        //         new Role("Manager"));
        //     await context.SaveChangesAsync();
        //     logger.LogInformation("Seeded roles.");
        // }

        await Task.CompletedTask;
    }
}
