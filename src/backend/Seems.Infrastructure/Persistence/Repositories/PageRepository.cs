using Microsoft.EntityFrameworkCore;
using Seems.Domain.Entities;
using Seems.Domain.Enums;
using Seems.Domain.Interfaces;

namespace Seems.Infrastructure.Persistence.Repositories;

public class PageRepository(AppDbContext context) : Repository<Page>(context), IPageRepository
{
    /// <summary>Looks up a page by its full computed path (e.g. "company/careers").</summary>
    public async Task<Page?> GetBySlugAsync(string path, CancellationToken ct = default)
        => await DbSet
            .Include(p => p.Slots.OrderBy(s => s.Order))
            .Include(p => p.Parent)
            .FirstOrDefaultAsync(p => p.Path == path, ct);

    /// <summary>Looks up a published page by its full computed path.</summary>
    public async Task<Page?> GetPublishedBySlugAsync(string path, CancellationToken ct = default)
        => await DbSet
            .Include(p => p.Slots.OrderBy(s => s.Order))
            .Include(p => p.Parent)
            .FirstOrDefaultAsync(p => p.Path == path && p.Status == ContentStatus.Published, ct);

    public async Task<IReadOnlyList<Page>> GetPublishedPagesAsync(CancellationToken ct = default)
        => await DbSet
            .Where(p => p.Status == ContentStatus.Published)
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<Page> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = DbSet.Include(p => p.Parent).OrderByDescending(p => p.UpdatedAt);
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
            .Include(p => p.Parent)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Page?> GetDefaultAsync(CancellationToken ct = default)
        => await DbSet
            .Include(p => p.Slots.OrderBy(s => s.Order))
            .Include(p => p.Parent)
            .FirstOrDefaultAsync(p => p.IsDefault, ct);

    public async Task<IReadOnlyList<Page>> GetAllForTreeAsync(CancellationToken ct = default)
        => await DbSet
            .Include(p => p.Parent)
            .OrderBy(p => p.ParentId == null ? 0 : 1)
            .ThenBy(p => p.SortOrder)
            .ThenBy(p => p.Title)
            .ToListAsync(ct);

    public async Task<bool> HasChildrenAsync(Guid id, CancellationToken ct = default)
        => await DbSet.AnyAsync(p => p.ParentId == id, ct);

    public async Task<IReadOnlyList<Page>> GetDescendantsAsync(Guid id, CancellationToken ct = default)
    {
        var ancestor = await DbSet.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (ancestor is null) return [];

        var pathPrefix = ancestor.Path + "/";
        return await DbSet
            .Where(p => p.Path.StartsWith(pathPrefix))
            .ToListAsync(ct);
    }
}
