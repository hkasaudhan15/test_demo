// ═══════════════════════════════════════════════════════
//  Database Migrator — standalone EF Core migration runner
//  Usage: dotnet run --project src/External/Migrator
//  Or:    dotnet ef migrations add <Name> --project src/External/Migrator
// ═══════════════════════════════════════════════════════

using CleanArch.Infrastructure.Persistence.EFCore.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

var app = builder.Build();

if (args.Contains("--migrate"))
{
    Console.WriteLine("🔄 Applying pending migrations...");
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    Console.WriteLine("✅ Migrations applied successfully.");
}
else
{
    Console.WriteLine("CleanArch.Migrator — EF Core Migration Host");
    Console.WriteLine("  --migrate    Apply pending migrations");
    Console.WriteLine("  Use 'dotnet ef' commands for migration management.");
}
