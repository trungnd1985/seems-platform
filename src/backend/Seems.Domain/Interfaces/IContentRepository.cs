using Seems.Domain.Entities;

namespace Seems.Domain.Interfaces;

public interface IContentRepository : IRepository<ContentItem>
{
    Task<IReadOnlyList<ContentItem>> GetByContentTypeKeyAsync(string contentTypeKey, CancellationToken ct = default);
}
