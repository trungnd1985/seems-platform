namespace Seems.Domain.Interfaces;

public record StorageUploadResult(string StorageKey, string PublicUrl);

public interface IStorageProvider
{
    string ProviderKey { get; }
    Task<StorageUploadResult> UploadAsync(byte[] content, string fileName, string contentType, Guid userId, CancellationToken ct = default);
    Task DeleteAsync(string storageKey, CancellationToken ct = default);
    string GetPublicUrl(string storageKey);
}
