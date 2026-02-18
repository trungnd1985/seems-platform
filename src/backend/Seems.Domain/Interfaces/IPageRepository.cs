using Seems.Domain.Entities;

namespace Seems.Domain.Interfaces;

public interface IPageRepository : IRepository<Page>
{
    Task<Page?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<Page>> GetPublishedPagesAsync(CancellationToken ct = default);
}
