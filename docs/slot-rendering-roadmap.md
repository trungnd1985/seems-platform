# Slot Rendering — Limitations & Roadmap

## Current Model

Each page slot maps to exactly one target:

```
SlotMapping {
  slotKey:    string          // which template slot
  targetType: 'Content' | 'Module'
  targetId:   string          // content item ID or module key
  order:      number
}
```

Content rendering resolves a single Vue component per content type key:

```ts
// utils/content-type-map.ts
const contentTypeMap: Record<string, Component> = {
  'rich-text':   RichText,
  'hero-block':  HeroBlock,
  'image-block': ImageBlock,
}
```

---

## Identified Limitations

### 1. No view mode per slot

The same content type always renders the same way regardless of context.
Example: an `article` content item always renders as a full detail page — there is no way to render it as a card, a teaser, or a list row.

### 2. No dynamic list slots

A slot always points to **one specific content item by ID**.
There is no way to create a slot that shows "all published articles" or "latest 5 posts in category X".
Creating one DB page per article to build a blog listing is impractical.

### 3. No content-type routing

Every page must be explicitly created in the DB with a slug.
There is no convention for `/articles/:slug` → auto-render the article with that slug.
Dynamic content detail pages (blog posts, product pages) cannot be supported with the current page model.

---

## Proposed Solutions

### Problem 1 — `viewMode` on `SlotMapping` *(small, backward-compatible)*

Add an optional `viewMode` string to `SlotMapping`.
The component resolver tries `${contentTypeKey}:${viewMode}` first, falls back to `${contentTypeKey}`.

**Backend changes:**
- Add `ViewMode string?` to `PageSlot` entity and migration
- Expose in `SlotMappingDto`, `AddPageSlotCommand`

**Frontend changes:**
- Add `viewMode?: string` to `SlotMapping` type
- Update `resolveContentComponent(key, viewMode?)` in `content-type-map.ts`
- Update `ContentRenderer` to receive and pass `viewMode`
- Update `SlotRenderer` to pass `viewMode` to `ContentRenderer`
- Update `PageSlotEditor` in admin to allow selecting `viewMode` when adding a slot

**Registration convention:**
```ts
'article':         ArticleDetail,   // default fallback
'article:card':    ArticleCard,
'article:detail':  ArticleDetail,
'article:teaser':  ArticleTeaser,
```

---

### Problem 2 — Dynamic lists: use Modules *(no schema change needed)*

A slot that shows "latest N articles" is inherently dynamic — it does its own querying.
This is exactly what **Modules** are designed for in this architecture.

**Approach:**
- Create an `article-list` module with its own Nuxt component
- The component fetches from `/api/modules/article-list?limit=10&tag=X` internally
- The slot simply maps to `targetType: Module, targetId: 'article-list'`

**Module component example (Nuxt):**
```vue
<!-- modules/article-list/PublicComponent.vue -->
<script setup lang="ts">
const props = defineProps<{ moduleKey: string }>()
const { data: articles } = await useFetch(`/api/modules/${props.moduleKey}`)
</script>
```

This keeps page composition declarative while allowing each module to own its data-fetching logic.

---

### Problem 3 — Content-type routing for detail pages *(larger feature)*

**Goal:** `/articles/my-post` renders the article with slug `my-post` without needing a dedicated page record.

**Required changes:**

1. **`ContentItem` gains a `Slug` field** (optional, unique within a content type)
2. **New API endpoint:** `GET /content-items/by-slug/{contentTypeKey}/{slug}`
3. **Nuxt fallback in `[...slug].vue`:**
   - If no `Page` is found for the slug, check if the first path segment matches a registered content-type route prefix (e.g., `articles` → `article`)
   - If a matching `ContentItem` is found, render using the content type's registered template
4. **Content-type → template mapping** (new config, e.g., `nuxt.config.ts` or a dedicated file):
   ```ts
   const contentTypeRoutes: Record<string, string> = {
     'article': 'BlogPost',   // template key
     'product': 'ProductPage',
   }
   ```

**Routing resolution order in `[...slug].vue`:**
```
1. Exact page slug match       → render page with its template
2. Content-type route match    → render content item with its mapped template
3. 404
```

---

## Implementation Priority

| # | Feature | Effort | Impact |
|---|---------|--------|--------|
| 1 | `viewMode` on `SlotMapping` | Small | Medium — enables card/detail/teaser variants |
| 2 | Dynamic list via Modules | Medium | High — unblocks blog list, category pages |
| 3 | Content-type routing | Large | High — unblocks scalable detail pages |
