using System.Text;
using System.Threading.RateLimiting;
using CleanArch.Application.Abstractions.Authentication;
using CleanArch.Presentation.Api.Middleware.ExceptionHandling;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace CleanArch.Presentation.Api.Extensions;

/// <summary>
/// Presentation-layer DI registration.
/// Call services.AddPresentation(configuration) from Program.cs.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Controllers ─────────────────────────────────
        services.AddControllers(options =>
        {
            // Suppress default model validation; FluentValidation handles it
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
        });

        // ── Current User ────────────────────────────────
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // ── JWT Authentication ──────────────────────────
        // Read JWT config directly from IConfiguration — no Infrastructure coupling
        var jwtSection = configuration.GetSection("JwtSettings");
        var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey is required.");
        var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("JwtSettings:Issuer is required.");
        var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("JwtSettings:Audience is required.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        // ── API Versioning ──────────────────────────────
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // ── Swagger / OpenAPI ───────────────────────────
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CleanArch API",
                Version = "v1",
                Description = "Production-grade API built with Clean Architecture + CQRS"
            });

            options.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "CleanArch API",
                Version = "v2"
            });

            // JWT auth in Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // ── CORS ────────────────────────────────────────
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000"];

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", builder =>
            {
                builder
                    .WithOrigins(allowedOrigins)
                    .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });
        });

        // ── Rate Limiting (configurable via appsettings.json) ──
        var rateLimitSection = configuration.GetSection("RateLimiting");
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter("fixed", limiterOptions =>
            {
                limiterOptions.PermitLimit = rateLimitSection.GetValue("Fixed:PermitLimit", 100);
                limiterOptions.Window = TimeSpan.FromMinutes(rateLimitSection.GetValue("Fixed:WindowMinutes", 1));
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = rateLimitSection.GetValue("Fixed:QueueLimit", 10);
            });

            options.AddSlidingWindowLimiter("sliding", limiterOptions =>
            {
                limiterOptions.PermitLimit = rateLimitSection.GetValue("Sliding:PermitLimit", 50);
                limiterOptions.Window = TimeSpan.FromMinutes(rateLimitSection.GetValue("Sliding:WindowMinutes", 1));
                limiterOptions.SegmentsPerWindow = rateLimitSection.GetValue("Sliding:SegmentsPerWindow", 6);
            });
        });

        // ── Response Compression ────────────────────────
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        // Health checks are registered in Infrastructure.DependencyInjection

        return services;
    }
}
