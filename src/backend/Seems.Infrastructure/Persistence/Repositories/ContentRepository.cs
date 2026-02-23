using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Seems.Domain.Entities;
using Seems.Domain.Enums;
using Seems.Domain.Interfaces;

namespace Seems.Infrastructure.Persistence.Repositories;

public class ContentRepository(AppDbContext context) : Repository<ContentItem>(context), IContentRepository
{
    // Field names come from the admin-defined ContentType schema (stored in DB).
    // Validate them before embedding in raw SQL as a defence-in-depth measure.
    private static readonly Regex FieldNameRegex =
        new(@"^[a-z][a-z0-9_]{0,62}$", RegexOptions.Compiled);

    private static readonly HashSet<string> SearchableTypes =
        new(StringComparer.OrdinalIgnoreCase) { "text", "textarea" };

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
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        // When a search term is supplied, start from a raw SQL base so we can use
        // jsonb path extraction (data->>'field' ILIKE ...) which has no LINQ translation.
        // Other filters are composed as LINQ on top — EF Core wraps FromSqlRaw in a subquery.
        IQueryable<ContentItem> query;
        if (!string.IsNullOrEmpty(search))
        {
            var (sql, sqlParams) = BuildSearchSql(search, contentTypeKey);
            query = DbSet.FromSqlRaw(sql, sqlParams)
                         .Include(c => c.ContentItemCategories);
        }
        else
        {
            query = DbSet.Include(c => c.ContentItemCategories).AsQueryable();
        }

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

    /// <summary>
    /// Builds a raw SQL SELECT with a ILIKE WHERE clause for full-text search.
    /// Field names are validated against a strict regex before use.
    /// Search values are always passed as SQL parameters.
    /// </summary>
    private (string Sql, object[] Parameters) BuildSearchSql(string search, string? contentTypeKey)
    {
        var escaped = $"%{EscapeILike(search)}%";

        if (!string.IsNullOrEmpty(contentTypeKey))
        {
            var contentType = context.Set<ContentType>()
                .AsNoTracking()
                .FirstOrDefault(c => c.Key == contentTypeKey);

            var fields = GetSearchableFields(contentType?.Schema);
            if (fields.Count > 0)
            {
                // Embed validated field names as SQL literals; values are NpgsqlParameters.
                // e.g. "Data"->>'title' ILIKE @p0 OR "Data"->>'body' ILIKE @p1
                var conditions = fields
                    .Select((f, i) => $"\"Data\"->>'{f}' ILIKE @p{i}")
                    .ToArray();
                var sql = $"SELECT * FROM \"ContentItems\" WHERE {string.Join(" OR ", conditions)}";
                var parameters = fields
                    .Select((_, i) => (object)new NpgsqlParameter($"p{i}", escaped))
                    .ToArray();
                return (sql, parameters);
            }
        }

        // Fallback: no searchable fields declared — scan entire JSON document as text.
        return (
            "SELECT * FROM \"ContentItems\" WHERE \"Data\"::text ILIKE @p0",
            [(object)new NpgsqlParameter("p0", escaped)]
        );
    }

    private static IReadOnlyList<string> GetSearchableFields(string? schema)
    {
        if (string.IsNullOrEmpty(schema)) return [];
        try
        {
            var doc = JsonDocument.Parse(schema);
            if (!doc.RootElement.TryGetProperty("fields", out var fieldsEl)) return [];

            var result = new List<string>();
            foreach (var field in fieldsEl.EnumerateArray())
            {
                if (!field.TryGetProperty("searchable", out var searchableProp)
                    || searchableProp.ValueKind != JsonValueKind.True)
                    continue;

                if (!field.TryGetProperty("name", out var nameProp)) continue;
                if (!field.TryGetProperty("type", out var typeProp)) continue;

                var name = nameProp.GetString() ?? "";
                var type = typeProp.GetString() ?? "";

                // Double-check name conforms to the same regex the frontend enforces.
                if (FieldNameRegex.IsMatch(name) && SearchableTypes.Contains(type))
                    result.Add(name);
            }
            return result;
        }
        catch
        {
            return [];
        }
    }

    private static string EscapeILike(string input) =>
        input.Replace(@"\", @"\\")
             .Replace("%", @"\%")
             .Replace("_", @"\_");
}
