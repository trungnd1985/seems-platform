using Seems.Domain.Entities;

namespace Seems.Domain.Interfaces;

public interface IPageRepository : IRepository<Page>
{
    /// <summary>Looks up a page by its full computed path (e.g. "company/careers").</summary>
    Task<Page?> GetBySlugAsync(string path, CancellationToken ct = default);
    /// <summary>Looks up a published page by its full computed path.</summary>
    Task<Page?> GetPublishedBySlugAsync(string path, CancellationToken ct = default);
    Task<IReadOnlyList<Page>> GetPublishedPagesAsync(CancellationToken ct = default);
    Task<(IReadOnlyList<Page> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<Page?> GetWithSlotsAsync(Guid id, CancellationToken ct = default);
    Task<Page?> GetDefaultAsync(CancellationToken ct = default);

    // Hierarchy
    Task<IReadOnlyList<Page>> GetAllForTreeAsync(CancellationToken ct = default);
    Task<bool> HasChildrenAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Page>> GetDescendantsAsync(Guid id, CancellationToken ct = default);
}
