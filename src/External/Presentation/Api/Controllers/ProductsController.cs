using CleanArch.Application.Features.Products.Commands.Create;
using CleanArch.Application.Features.Products.Queries.GetById;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CleanArch.Presentation.Api.Controllers;

/// <summary>
/// Products API — demonstrates the complete domain event flow:
///
///   POST /api/v1/products
///     → CreateProductCommand → Handler creates Product
///     → Product.Create() raises ProductCreatedDomainEvent
///     → SaveChanges → OutboxInterceptor serializes event to OutboxMessage
///     → OutboxProcessor (background) publishes via MediatR
///     → ProductCreatedDomainEventHandler logs / sends notifications
///
///   GET /api/v1/products/{id}
///     → GetProductByIdQuery → Handler reads from repository
/// </summary>
public sealed class ProductsController : BaseApiController
{
    /// <summary>
    /// Creates a new product. Raises ProductCreatedDomainEvent via outbox.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var result = await Mediator.Send(command);

        return HandleCreated(result, nameof(GetById), new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetProductByIdQuery(id));

        return HandleResult(result);
    }
}
