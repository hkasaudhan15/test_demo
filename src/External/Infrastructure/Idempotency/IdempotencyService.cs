using CleanArch.Application.Abstractions.Idempotency;
using CleanArch.CrossCutting.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanArch.Infrastructure.Idempotency;

/// <summary>
/// EF Core implementation of <see cref="IIdempotencyService"/>.
/// Uses the <see cref="IdempotentRequest"/> table to track processed keys.
///
/// The PK constraint on <c>IdempotentRequest.Id</c> (= idempotency key)
/// provides the final guarantee against duplicates even under race conditions.
/// </summary>
public sealed class IdempotencyService : IIdempotencyService
{
    private readonly DbContext _dbContext;
    private readonly ILogger<IdempotencyService> _logger;

    public IdempotencyService(
        DbContext dbContext,
        ILogger<IdempotencyService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> ExistsAsync(Guid idempotencyKey, CancellationToken ct = default)
    {
        return await _dbContext.Set<IdempotentRequest>()
            .AnyAsync(r => r.Id == idempotencyKey, ct);
    }

    public async Task RecordAsync(Guid idempotencyKey, string commandName, CancellationToken ct = default)
    {
        try
        {
            var request = IdempotentRequest.Create(idempotencyKey, commandName, DateTime.UtcNow);
            _dbContext.Set<IdempotentRequest>().Add(request);
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // PK violation — another request with the same key was inserted
            // concurrently. This is expected under race conditions and is safe
            // to swallow: the first request already succeeded.
            _logger.LogWarning(
                "Idempotent key {IdempotencyKey} was concurrently inserted for {CommandName}",
                idempotencyKey, commandName);
        }
    }
}
