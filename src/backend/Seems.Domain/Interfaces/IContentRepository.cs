using Seems.Domain.Entities;
using Seems.Domain.Enums;

namespace Seems.Domain.Interfaces;

public interface IContentRepository : IRepository<ContentItem>
{
    Task<IReadOnlyList<ContentItem>> GetByContentTypeKeyAsync(string contentTypeKey, CancellationToken ct = default);
    Task<(IReadOnlyList<ContentItem> Items, int Total)> ListAsync(string? contentTypeKey, ContentStatus? status, Guid? categoryId, int page, int pageSize, CancellationToken ct = default);
}
