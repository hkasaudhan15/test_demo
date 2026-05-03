namespace CleanArch.SharedKernel.Pagination;

/// <summary>
/// Paginated response wrapper — generic, reusable for any list endpoint.
/// </summary>
public sealed class PagedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    private PagedList(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public static PagedList<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        return new PagedList<T>(items, page, pageSize, totalCount);
    }
}

/// <summary>
/// Pagination query parameters — include in CQRS queries.
/// </summary>
public sealed record PaginationParams
{
    private const int MaxPageSize = 100;

    public int Page { get; init; } = 1;

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public string? SearchTerm { get; init; }
}
