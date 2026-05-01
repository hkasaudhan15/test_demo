using CleanArch.SharedKernel.Constants;

namespace CleanArch.Application.Abstractions.Pagination;

/// <summary>
/// Reusable pagination parameters for paginated queries.
/// Provides validation and defaults from <see cref="AppConstants.Pagination"/>.
///
/// Embed this in your query record:
///   public sealed record GetOrdersQuery(PaginationParams Pagination) : IQuery&lt;PaginatedResult&lt;OrderDto&gt;&gt;;
///
/// Or use the individual properties directly on your query:
///   public sealed record GetOrdersQuery(int PageNumber, int PageSize) : IQuery&lt;...&gt;;
/// </summary>
public sealed record PaginationParams
{
    private readonly int _pageNumber;
    private readonly int _pageSize;

    public PaginationParams(int pageNumber = AppConstants.Pagination.DefaultPage, int pageSize = AppConstants.Pagination.DefaultPageSize)
    {
        _pageNumber = pageNumber < 1 ? AppConstants.Pagination.DefaultPage : pageNumber;
        _pageSize = pageSize < 1 ? AppConstants.Pagination.DefaultPageSize :
                    pageSize > AppConstants.Pagination.MaxPageSize ? AppConstants.Pagination.MaxPageSize : pageSize;
    }

    public int PageNumber => _pageNumber;
    public int PageSize => _pageSize;

    /// <summary>
    /// Number of records to skip for the current page.
    /// Use this when building specifications: spec.ApplyPaging(pagination.Skip, pagination.PageSize)
    /// </summary>
    public int Skip => (_pageNumber - 1) * _pageSize;
}
