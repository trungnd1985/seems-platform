using Seems.Domain.Entities;

namespace Seems.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<Category>> GetByContentTypeKeyAsync(string contentTypeKey, CancellationToken ct = default);
    Task SyncContentItemCategoriesAsync(Guid contentItemId, IEnumerable<Guid> categoryIds, CancellationToken ct = default);
}
