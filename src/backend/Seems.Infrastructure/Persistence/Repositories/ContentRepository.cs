using Microsoft.EntityFrameworkCore;
using Seems.Domain.Entities;
using Seems.Domain.Enums;
using Seems.Domain.Interfaces;

namespace Seems.Infrastructure.Persistence.Repositories;

public class ContentRepository(AppDbContext context) : Repository<ContentItem>(context), IContentRepository
{
    public override async Task<ContentItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Include(c => c.ContentItemCategories)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<ContentItem>> GetByContentTypeKeyAsync(string contentTypeKey,
        CancellationToken ct = default)
        => await DbSet
            .Where(c => c.ContentTypeKey == contentTypeKey)
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<ContentItem> Items, int Total)> ListAsync(
        string? contentTypeKey,
        ContentStatus? status,
        Guid? categoryId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = DbSet.Include(c => c.ContentItemCategories).AsQueryable();

        if (!string.IsNullOrEmpty(contentTypeKey))
            query = query.Where(c => c.ContentTypeKey == contentTypeKey);

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (categoryId.HasValue)
            query = query.Where(c => c.ContentItemCategories.Any(cc => cc.CategoryId == categoryId.Value));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}
