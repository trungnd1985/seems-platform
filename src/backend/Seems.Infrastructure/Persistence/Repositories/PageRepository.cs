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

    /// <inheritdoc/>
    public async Task<(Page Page, Dictionary<string, string> UrlParams)?> ResolveByPathAsync(
        string requestPath, bool publishedOnly, CancellationToken ct = default)
    {
        var baseQuery = DbSet
            .Include(p => p.Slots.OrderBy(s => s.Order))
            .Include(p => p.Parent);

        // 1. Exact match (fast path — avoids loading all parametric pages)
        var exact = publishedOnly
            ? await baseQuery.FirstOrDefaultAsync(p => p.Path == requestPath && p.Status == ContentStatus.Published, ct)
            : await baseQuery.FirstOrDefaultAsync(p => p.Path == requestPath, ct);

        if (exact is not null)
            return (exact, []);

        // 2. Pattern match — load pages whose path contains ':' (parametric templates)
        var candidates = publishedOnly
            ? await baseQuery.Where(p => p.Path.Contains(':') && p.Status == ContentStatus.Published).ToListAsync(ct)
            : await baseQuery.Where(p => p.Path.Contains(':')).ToListAsync(ct);

        var requestSegments = requestPath.Split('/', StringSplitOptions.None);

        // Sort by specificity: fewer param segments = more specific = tried first
        var ordered = candidates.OrderBy(p => p.Path.Count(c => c == ':'));

        foreach (var candidate in ordered)
        {
            var patternSegments = candidate.Path.Split('/', StringSplitOptions.None);
            if (patternSegments.Length != requestSegments.Length) continue;

            var urlParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var matched = true;

            for (var i = 0; i < patternSegments.Length; i++)
            {
                var pattern = patternSegments[i];
                if (pattern.StartsWith(':'))
                    urlParams[pattern[1..]] = requestSegments[i];
                else if (!string.Equals(pattern, requestSegments[i], StringComparison.OrdinalIgnoreCase))
                { matched = false; break; }
            }

            if (matched) return (candidate, urlParams);
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Page>> GetPublishedPagesAsync(CancellationToken ct = default)
        => await DbSet
            .Where(p => p.Status == ContentStatus.Published)
            .OrderBy(p => p.SortOrder)
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
