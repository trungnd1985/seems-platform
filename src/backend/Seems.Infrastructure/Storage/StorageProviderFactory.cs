using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Seems.Domain.Interfaces;
using Seems.Infrastructure.Persistence;

namespace Seems.Infrastructure.Storage;

public class StorageProviderFactory(
    AppDbContext db,
    IEnumerable<IStorageProvider> providers) : IStorageProviderFactory
{
    private readonly Dictionary<string, IStorageProvider> _providers =
        providers.ToDictionary(p => p.ProviderKey, StringComparer.OrdinalIgnoreCase);

    public async Task<IStorageProvider> GetCurrentAsync(CancellationToken ct = default)
    {
        var setting = await db.SiteSettings
            .FirstOrDefaultAsync(s => s.Key == "storage", ct);

        if (setting is not null)
        {
            try
            {
                var doc = JsonDocument.Parse(setting.Value);
                if (doc.RootElement.TryGetProperty("provider", out var providerEl))
                {
                    var key = providerEl.GetString() ?? "local";
                    if (_providers.TryGetValue(key, out var matched))
                        return matched;
                }
            }
            catch (JsonException) { /* fall through to default */ }
        }

        return _providers["local"];
    }
}
