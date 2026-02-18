using Microsoft.EntityFrameworkCore;
using Seems.Domain.Entities;
using Seems.Domain.Enums;
using Seems.Domain.Interfaces;

namespace Seems.Infrastructure.Persistence.Repositories;

public class PageRepository(AppDbContext context) : Repository<Page>(context), IPageRepository
{
    public async Task<Page?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => await DbSet
            .Include(p => p.Slots.OrderBy(s => s.Order))
            .FirstOrDefaultAsync(p => p.Slug == slug, ct);

    public async Task<IReadOnlyList<Page>> GetPublishedPagesAsync(CancellationToken ct = default)
        => await DbSet
            .Where(p => p.Status == ContentStatus.Published)
            .ToListAsync(ct);
}
