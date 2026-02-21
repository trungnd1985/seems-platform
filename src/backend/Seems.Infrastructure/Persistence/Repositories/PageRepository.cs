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

    public async Task<(IReadOnlyList<Page> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = DbSet.OrderByDescending(p => p.UpdatedAt);
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<Page?> GetWithSlotsAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Include(p => p.Slots.OrderBy(s => s.Order))
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Page?> GetDefaultAsync(CancellationToken ct = default)
        => await DbSet
            .Include(p => p.Slots.OrderBy(s => s.Order))
            .FirstOrDefaultAsync(p => p.IsDefault, ct);
}
