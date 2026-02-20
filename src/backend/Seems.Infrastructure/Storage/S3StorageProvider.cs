using Seems.Domain.Interfaces;

namespace Seems.Infrastructure.Storage;

/// <summary>
/// Stub implementation for S3-compatible storage (AWS S3, MinIO, Cloudflare R2).
/// Configure and implement when the S3 provider is needed.
/// </summary>
public class S3StorageProvider : IStorageProvider
{
    public string ProviderKey => "s3";

    public Task<StorageUploadResult> UploadAsync(byte[] content, string fileName, string contentType, Guid userId, CancellationToken ct = default)
        => throw new NotImplementedException("S3 storage provider is not yet configured.");

    public Task DeleteAsync(string storageKey, CancellationToken ct = default)
        => throw new NotImplementedException("S3 storage provider is not yet configured.");

    public string GetPublicUrl(string storageKey)
        => throw new NotImplementedException("S3 storage provider is not yet configured.");
}
