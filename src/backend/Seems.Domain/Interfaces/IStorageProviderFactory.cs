namespace Seems.Domain.Interfaces;

public interface IStorageProviderFactory
{
    Task<IStorageProvider> GetCurrentAsync(CancellationToken ct = default);
}
