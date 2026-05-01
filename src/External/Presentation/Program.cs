using CleanArch.Application;
using CleanArch.CrossCutting.MultiTenancy;
using CleanArch.Infrastructure;
using CleanArch.Presentation.Api.Extensions;
using CleanArch.Presentation.Api.Middleware.Correlation;
using CleanArch.Presentation.Api.Middleware.ExceptionHandling;
using CleanArch.Presentation.Api.Middleware.RequestLogging;
using Serilog;

// ════════════════════════════════════════════════════════════
//  Bootstrap — Serilog two-stage initialization
// ════════════════════════════════════════════════════════════

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting CleanArch API...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ─────────────────────────────────────────
    builder.Host.UseSerilog((context, loggerConfig) =>
        loggerConfig.ReadFrom.Configuration(context.Configuration));

    // ── Layer DI Registration ───────────────────────────
    builder.Services
        .AddApplication()                              // Core: MediatR, FluentValidation, AutoMapper
        .AddInfrastructure(builder.Configuration)      // External: EF Core, Redis, JWT, etc.
        .AddPresentation(builder.Configuration);       // External: Controllers, Swagger, CORS, etc.

    var app = builder.Build();

    // ════════════════════════════════════════════════════
    //  Middleware Pipeline (ORDER MATTERS)
    // ════════════════════════════════════════════════════

    // 1. Correlation ID — first, so every log entry has it
    app.UseMiddleware<CorrelationIdMiddleware>();

    // 2. Tenant resolution — resolve tenant from header before anything else
    app.UseMiddleware<TenantResolutionMiddleware>();

    // 3. Global exception handler — catches everything below
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    // 4. Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = RequestLoggingEnricher.EnrichFromRequest;
    });

    // 5. HTTPS redirect
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }
    app.UseHttpsRedirection();

    // 6. Response compression
    app.UseResponseCompression();

    // 7. CORS
    app.UseCors("DefaultPolicy");

    // 8. Rate limiting
    app.UseRateLimiter();

    // 9. Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // 10. Swagger (dev/staging only)
    if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "CleanArch API v1");
            options.SwaggerEndpoint("/swagger/v2/swagger.json", "CleanArch API v2");
            options.RoutePrefix = string.Empty; // Serve at root
        });
    }

    // 11. Health checks
    app.MapHealthChecks("/health");

    // 12. Map controllers
    app.MapControllers();

    // ── Run ─────────────────────────────────────────────
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.Information("Application shutting down...");
    Log.CloseAndFlush();
}

// Required for WebApplicationFactory in integration tests
public partial class Program;
