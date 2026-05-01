using System.Net;
using System.Text.Json;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.Primitives.Exceptions;

namespace CleanArch.Presentation.Api.Middleware.ExceptionHandling;

/// <summary>
/// Global exception handler — catches ALL unhandled exceptions
/// and maps them to proper HTTP status codes with consistent response shape.
/// </summary>
public sealed class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            BusinessRuleException e => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(e.Message)),

            ConcurrencyException e => (
                HttpStatusCode.Conflict,
                ApiResponse<object>.Fail(e.Message)),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                ApiResponse<object>.Fail("You are not authenticated.")),

            ArgumentException e => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(e.Message)),

            KeyNotFoundException e => (
                HttpStatusCode.NotFound,
                ApiResponse<object>.Fail(e.Message)),

            OperationCanceledException => (
                HttpStatusCode.RequestTimeout,
                ApiResponse<object>.Fail("The request was cancelled.")),

            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail("An unexpected error occurred. Please try again later."))
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response, JsonOptions);
        await context.Response.WriteAsync(json);
    }
}
