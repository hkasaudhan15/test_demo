using CleanArch.Application;
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
    Log.Information("🚀 Starting CleanArch API...");

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

    // 2. Global exception handler — catches everything below
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    // 3. Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = RequestLoggingEnricher.EnrichFromRequest;
    });

    // 4. HTTPS redirect
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }
    app.UseHttpsRedirection();

    // 5. Response compression
    app.UseResponseCompression();

    // 6. CORS
    app.UseCors("DefaultPolicy");

    // 7. Rate limiting
    app.UseRateLimiter();

    // 8. Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // 9. Swagger (dev/staging only)
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

    // 10. Health checks
    app.MapHealthChecks("/health");

    // 11. Map controllers
    app.MapControllers();

    // ── Run ─────────────────────────────────────────────
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "💀 Application terminated unexpectedly");
}
finally
{
    Log.Information("🛑 Application shutting down...");
    Log.CloseAndFlush();
}

// Required for WebApplicationFactory in integration tests
public partial class Program;
