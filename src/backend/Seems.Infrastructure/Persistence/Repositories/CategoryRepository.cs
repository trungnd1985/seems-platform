using Microsoft.EntityFrameworkCore;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Infrastructure.Persistence.Repositories;

public class CategoryRepository(AppDbContext context) : Repository<Category>(context), ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetByContentTypeKeyAsync(string contentTypeKey, CancellationToken ct = default)
        => await DbSet
            .Include(c => c.ContentItemCategories)
            .Where(c => c.ContentTypeKey == contentTypeKey)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);

    public async Task SyncContentItemCategoriesAsync(Guid contentItemId, IEnumerable<Guid> categoryIds, CancellationToken ct = default)
    {
        var existing = await context.ContentItemCategories
            .Where(x => x.ContentItemId == contentItemId)
            .ToListAsync(ct);

        context.ContentItemCategories.RemoveRange(existing);

        var newIds = categoryIds.ToList();
        if (newIds.Count > 0)
        {
            context.ContentItemCategories.AddRange(
                newIds.Select(cid => new ContentItemCategory
                {
                    ContentItemId = contentItemId,
                    CategoryId = cid,
                }));
        }
    }
}
