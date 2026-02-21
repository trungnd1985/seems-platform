using Seems.Domain.Entities;

namespace Seems.Domain.Interfaces;

public interface IPageRepository : IRepository<Page>
{
    Task<Page?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Page?> GetPublishedBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<Page>> GetPublishedPagesAsync(CancellationToken ct = default);
    Task<(IReadOnlyList<Page> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<Page?> GetWithSlotsAsync(Guid id, CancellationToken ct = default);
    Task<Page?> GetDefaultAsync(CancellationToken ct = default);
}
