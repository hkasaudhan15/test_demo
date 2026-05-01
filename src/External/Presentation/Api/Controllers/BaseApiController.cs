using Asp.Versioning;
using CleanArch.Application.Common.Models;
using CleanArch.Domain.Primitives.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanArch.Presentation.Api.Controllers;

/// <summary>
/// Base API controller — provides MediatR, Result-to-ActionResult mapping,
/// and consistent response patterns. All controllers inherit from this.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// Maps a Result to an appropriate ActionResult.
    /// Success → 200 OK, Failure → appropriate error status.
    /// </summary>
    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse<object>.Ok(null!, "Operation completed successfully."));

        return HandleError(result.Error);
    }

    /// <summary>
    /// Maps a Result<T> to an appropriate ActionResult with data.
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse<T>.Ok(result.Value));

        return HandleError(result.Error);
    }

    /// <summary>
    /// Maps a Result<T> to 201 Created with location header.
    /// </summary>
    protected IActionResult HandleCreated<T>(Result<T> result, string actionName, object routeValues)
    {
        if (result.IsSuccess)
            return CreatedAtAction(actionName, routeValues, ApiResponse<T>.Ok(result.Value, "Created successfully."));

        return HandleError(result.Error);
    }

    private IActionResult HandleError(Error error)
    {
        return error.Type switch
        {
            ErrorType.Validation when error is ValidationError validationError =>
                BadRequest(ApiResponse<object>.Fail("Validation failed.",
                    validationError.Errors.ToDictionary(
                        e => e.Code,
                        e => new[] { e.Message }))),
            ErrorType.NotFound => NotFound(ApiResponse<object>.Fail(error.Message)),
            ErrorType.Unauthorized => Unauthorized(ApiResponse<object>.Fail(error.Message)),
            ErrorType.Forbidden => Forbid(),
            ErrorType.Conflict => Conflict(ApiResponse<object>.Fail(error.Message)),
            _ => BadRequest(ApiResponse<object>.Fail(error.Message))
        };
    }
}

