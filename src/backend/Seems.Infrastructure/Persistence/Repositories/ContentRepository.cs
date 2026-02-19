using Microsoft.EntityFrameworkCore;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Infrastructure.Persistence.Repositories;

public class ContentRepository(AppDbContext context) : Repository<ContentItem>(context), IContentRepository
{
    public async Task<IReadOnlyList<ContentItem>> GetByContentTypeKeyAsync(string contentTypeKey,
        CancellationToken ct = default)
        => await DbSet
            .Where(c => c.ContentTypeKey == contentTypeKey)
            .ToListAsync(ct);
}
