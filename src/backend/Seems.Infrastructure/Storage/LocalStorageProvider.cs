using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Seems.Domain.Interfaces;

namespace Seems.Infrastructure.Storage;

public class LocalStorageProvider(IWebHostEnvironment env, IConfiguration configuration) : IStorageProvider
{
    public string ProviderKey => "local";

    private string BaseUrl => configuration["Storage:Local:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5000";

    // WebRootPath is null when no wwwroot folder exists; fall back to ContentRootPath/wwwroot
    private string WebRoot => env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");

    public Task<StorageUploadResult> UploadAsync(byte[] content, string fileName, string contentType, Guid userId, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(fileName);
        var storageKey = $"media/{userId}/{Guid.NewGuid():N}{ext}";
        var absolutePath = Path.Combine(WebRoot, storageKey.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        File.WriteAllBytes(absolutePath, content);

        var publicUrl = $"{BaseUrl}/{storageKey}";
        return Task.FromResult(new StorageUploadResult(storageKey, publicUrl));
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        var absolutePath = Path.Combine(WebRoot, storageKey.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(absolutePath))
            File.Delete(absolutePath);
        return Task.CompletedTask;
    }

    public string GetPublicUrl(string storageKey) => $"{BaseUrl}/{storageKey}";
}
