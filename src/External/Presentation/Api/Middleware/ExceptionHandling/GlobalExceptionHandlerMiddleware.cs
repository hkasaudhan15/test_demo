using System.Net;
using System.Text.Json;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.Primitives.Exceptions;
using CleanArch.SharedKernel.Constants;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Presentation.Api.Middleware.ExceptionHandling;

/// <summary>
/// Global exception handler — catches ALL unhandled exceptions
/// and maps them to proper HTTP status codes with a consistent
/// <see cref="ApiResponse{T}"/> envelope that includes the correlation ID.
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
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected — no point in writing a response.
            _logger.LogInformation("Request was cancelled by the client");
            context.Response.StatusCode = 499; // nginx-style "Client Closed Request"
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            BusinessRuleException e => (HttpStatusCode.BadRequest, e.Message),
            ConcurrencyException e => (HttpStatusCode.Conflict, e.Message),
            DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "The record was modified by another user. Please refresh and try again."),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "You are not authenticated."),
            ArgumentException e => (HttpStatusCode.BadRequest, e.Message),
            KeyNotFoundException e => (HttpStatusCode.NotFound, e.Message),
            OperationCanceledException => (HttpStatusCode.RequestTimeout, "The request was cancelled."),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(message);
        var json = JsonSerializer.Serialize(response, JsonOptions);
        await context.Response.WriteAsync(json);
    }
}
