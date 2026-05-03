namespace CleanArch.Application.Common.Models;

/// <summary>
/// Standard API response envelope. ALL endpoints return this shape.
/// Ensures consistent response format across the entire API.
/// </summary>
public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public IDictionary<string, string[]>? Errors { get; init; }
    public ApiMetadata? Meta { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public static ApiResponse<T> Fail(string message, IDictionary<string, string[]>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors
    };

    public static ApiResponse<T> Paginated(T data, int page, int pageSize, int totalCount) => new()
    {
        Success = true,
        Data = data,
        Meta = new ApiMetadata { Page = page, PageSize = pageSize, TotalCount = totalCount }
    };
}

public sealed class ApiMetadata
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
