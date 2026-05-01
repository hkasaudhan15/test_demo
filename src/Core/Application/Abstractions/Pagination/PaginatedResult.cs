namespace CleanArch.Application.Abstractions.Pagination;

/// <summary>
/// Standard paginated response wrapper for list endpoints.
/// Contains the page of items plus metadata for client-side navigation.
///
/// Usage in query handlers:
///   return Result.Success(PaginatedResult&lt;OrderDto&gt;.Create(items, count, page, pageSize));
///
/// API consumers receive:
///   { "items": [...], "pageNumber": 1, "pageSize": 10, "totalCount": 42,
///     "totalPages": 5, "hasPreviousPage": false, "hasNextPage": true }
/// </summary>
public sealed class PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    private PaginatedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    public static PaginatedResult<T> Create(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        return new PaginatedResult<T>(items, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Returns an empty page (useful for early returns when no data matches).
    /// </summary>
    public static PaginatedResult<T> Empty(int pageNumber, int pageSize)
    {
        return new PaginatedResult<T>([], 0, pageNumber, pageSize);
    }
}
