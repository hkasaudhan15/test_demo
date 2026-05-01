namespace CleanArch.Application.Abstractions.Storage;

/// <summary>
/// File storage abstraction — supports local disk, Azure Blob, S3, etc.
/// </summary>
public interface IFileStorageService
{
    Task<string> UploadAsync(string containerName, string fileName, Stream stream, string contentType, CancellationToken ct = default);
    Task<Stream?> DownloadAsync(string containerName, string fileName, CancellationToken ct = default);
    Task DeleteAsync(string containerName, string fileName, CancellationToken ct = default);
    Task<string> GetPresignedUrlAsync(string containerName, string fileName, TimeSpan expiry, CancellationToken ct = default);
    Task<bool> ExistsAsync(string containerName, string fileName, CancellationToken ct = default);
}
