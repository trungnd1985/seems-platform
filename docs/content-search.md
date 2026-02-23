# Content Item Search

## Overview

Content Items store their fields as a `jsonb` document in PostgreSQL. Because field names are
defined dynamically by each ContentType schema, search cannot use ordinary indexed columns.
This document describes the implemented search strategy (Phase 1) and the planned upgrade path.

---

## Architecture

### Storage

| Aspect | Detail |
|---|---|
| Column type | `jsonb` (was `json` before migration `AddJsonbSearch`) |
| GIN index | `ix_content_items_data_gin` on `"ContentItems"."Data"` — accelerates `@>` containment queries |
| Table | `ContentItems` |

### Searchable fields

Which fields participate in search is declared per-field in the ContentType schema:

```json
{
  "fields": [
    { "name": "title",  "type": "text",     "searchable": true },
    { "name": "body",   "type": "textarea",  "searchable": true },
    { "name": "sku",    "type": "text",     "searchable": false },
    { "name": "price",  "type": "number"  }
  ]
}
```

Only `text` and `textarea` field types support `searchable: true`. Other types (`richtext`,
`number`, `boolean`, `datetime`, `media`, `relation`) are excluded because their stored values
are not meaningful for substring matching.

---

## How search works

### Request flow

```
Admin UI (InputText, 350 ms debounce)
  → GET /api/content-items?search=climate&contentTypeKey=blog
  → ListContentItemsController
  → ListContentItemsHandler  (enforces minimum 3-char term)
  → IContentRepository.ListAsync(search: "climate", ...)
  → ContentRepository.BuildSearchSql()
```

### Query building — `ContentRepository.BuildSearchSql`

When `contentTypeKey` is known:

1. Load the ContentType entity from DB (`AsNoTracking`).
2. Parse `ContentType.Schema` JSON with `System.Text.Json`.
3. Extract fields where `searchable == true` AND `type` is `text` or `textarea`.
4. Validate each field name against `^[a-z][a-z0-9_]{0,62}$` (defence in depth).
5. Build a raw SQL base query:

```sql
SELECT * FROM "ContentItems"
WHERE "Data"->>'title' ILIKE @p0
   OR "Data"->>'body'  ILIKE @p1
```

Each `@p0`, `@p1` … is a `NpgsqlParameter` — the search value is never interpolated into SQL.

When `contentTypeKey` is **not** supplied, or the ContentType has no searchable fields:

```sql
SELECT * FROM "ContentItems"
WHERE "Data"::text ILIKE @p0
```

This scans the full JSON document as text, including key names. It is intentionally broad and
is only used when no ContentType filter is active.

The raw SQL result is returned as a composable `IQueryable<ContentItem>` via
`DbSet.FromSqlRaw(...)`. All other LINQ filters (`contentTypeKey`, `status`, `categoryId`)
are then chained on top — EF Core wraps the raw SQL in a subquery automatically.

### ILIKE pattern escaping

User input is sanitised before building the `LIKE` pattern:

```csharp
private static string EscapeILike(string input) =>
    input.Replace(@"\", @"\\")
         .Replace("%", @"\%")
         .Replace("_", @"\_");
```

The resulting pattern is `%{escaped}%` (case-insensitive substring match).

---

## Security

| Risk | Mitigation |
|---|---|
| SQL injection via field names | Field names are sourced from the DB-stored ContentType schema and validated against `^[a-z][a-z0-9_]{0,62}$` before embedding in SQL |
| SQL injection via search term | Search term is always passed as an `NpgsqlParameter` — never interpolated |
| ILIKE metacharacters (`%`, `_`, `\`) | Escaped with `EscapeILike()` before pattern construction |
| DoS via short/expensive patterns | Minimum 3-character term enforced in `ListContentItemsHandler` (backend), not only in the UI |
| Scope | Endpoint is `[Authorize]` — unauthenticated callers cannot reach it |

---

## Frontend

### InputText with debounce

`ContentItemListView.vue` renders an `InputText` in the filters bar. Keystrokes are debounced
at **350 ms** before triggering a new fetch. The composable `useContentItems` skips sending
the `search` query parameter when the term is shorter than 3 characters.

### Marking fields as searchable

In the ContentType editor (**Content Types → edit → field editor**), `text` and `textarea`
fields expose a **Searchable** checkbox. When checked, `"searchable": true` is stored in the
schema JSON. The SchemaBuilder list shows a blue `searchable` tag next to those fields.

Fields edited or newly created after enabling search are picked up automatically — no migration
or server restart is required because the schema is read at query time.

---

## Performance characteristics

| Scenario | Query type | Index used |
|---|---|---|
| Exact JSON key/value match (`@>`) | jsonb containment | GIN index |
| Field-specific ILIKE (`data->>'title' ILIKE '%q%'`) | Sequential scan per row | None (needs `pg_trgm` for indexing) |
| Full-document ILIKE (`data::text ILIKE '%q%'`) | Sequential scan | None |

For admin use at typical CMS scale (< 50 k items) the sequential scan completes in under
100 ms. At larger scale, add `pg_trgm` expression indexes per searchable field (see Phase 2
below).

---

## Upgrade path

### Phase 2 — `pg_trgm` trigram indexes (when needed)

Enable the extension and create an expression index for each searchable field:

```sql
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Repeat for each searchable field:
CREATE INDEX CONCURRENTLY content_items_title_trgm
    ON "ContentItems" USING GIN(("Data"->>'title') gin_trgm_ops);
```

With trigram indexes, `ILIKE '%q%'` uses a bitmap index scan instead of a sequential scan.
These indexes can be created on-demand when a field is marked `searchable: true` in the
ContentType editor (trigger an `IDbConnection.ExecuteAsync` call in the update handler).

### Phase 3 — Public site full-text search (Meilisearch / Elasticsearch)

For the public-facing `/api/search` endpoint (used by Nuxt SSR pages):

- Index **published** ContentItems on write via a MediatR notification handler.
- Query Meilisearch / Elasticsearch from the public API endpoint.
- Admin search continues to use PostgreSQL — no need to couple the two concerns.
- Re-index job required when ContentType schema changes (field additions / removals).

This phase is only warranted once a public site search feature is needed.

---

## API reference

```
GET /api/content-items
  ?page=1
  &pageSize=20
  &contentTypeKey=blog       (optional — improves search precision)
  &status=Published          (optional)
  &categoryId={guid}         (optional)
  &search=climate            (optional — minimum 3 chars, case-insensitive)
```

Terms shorter than 3 characters are silently ignored server-side.
