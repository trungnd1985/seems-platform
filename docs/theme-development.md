# Theme Development Guide

## How a Theme Works

A theme in this platform is two things:

1. **A backend record** (key, name, cssUrl) — registered via the admin UI
2. **A CSS bundle** (external file served from CDN or static host)

Optionally, a theme can also **override template Vue components** for theme-specific layouts.

---

## Theme Development Flow

### Step 1 — Register the Theme (Admin)

Create a theme record via the admin UI or API:

```json
{
  "key": "my-theme",
  "name": "My Theme",
  "cssUrl": "https://cdn.example.com/themes/my-theme/style.css"
}
```

The `key` is used throughout the system as the stable identifier.

---

### Step 2 — Build the CSS Bundle

The CSS file is the core of your theme. The Nuxt frontend sets `data-theme="{themeKey}"` on the `<html>` element and injects your `cssUrl` as a `<link>` stylesheet.

**Scope all rules to your theme key:**

```css
/* themes/my-theme/style.css */

[data-theme="my-theme"] {
  --color-primary: #1a56db;
  --color-bg: #f9fafb;
  --color-text: #111827;
  --font-body: 'Inter', sans-serif;
  --font-heading: 'Playfair Display', serif;
}

[data-theme="my-theme"] body {
  font-family: var(--font-body);
  background: var(--color-bg);
  color: var(--color-text);
}

/* Slot-specific layout overrides */
[data-theme="my-theme"] .slot-hero {
  background: var(--color-primary);
  color: #fff;
  padding: 4rem 2rem;
}

[data-theme="my-theme"] .standard-page-body {
  max-width: 960px;
}
```

Scoping to `[data-theme]` ensures no bleed between themes and allows safe live-switching.

---

### Step 3 — (Optional) Override Templates

If your theme needs a different layout for a template (e.g., a full-width `standard-page` with no sidebar), add the override to `src/frontend/public/composables/useTemplateResolver.ts`:

```ts
// 1. Import the theme-specific component
import MyThemeStandardPage from '~/components/templates/my-theme/StandardPage.vue'

// 2. Register the override in themeTemplateMap
const themeTemplateMap: Record<string, Record<string, Component>> = {
  'my-theme': {
    'standard-page': MyThemeStandardPage,
  },
}
```

Your override component receives the same props as the base template:

```ts
// components/templates/my-theme/StandardPage.vue
defineProps<{
  page: Page
  slots: SlotMapping[]
}>()
```

It can render any slot layout it needs — `hero`, `main`, `sidebar`, `footer` — using the same `<SlotRenderer>` component.

---

## What a Theme Can and Cannot Do

| Can | Cannot |
|-----|--------|
| Inject a CSS stylesheet | Modify backend data |
| Override template Vue components | Define new slot keys (done in templates) |
| Scope styles to `[data-theme]` | Register module components |
| Use CSS custom properties | Change SEO behavior |
| Use web fonts via `@import` | Access server-side rendering context |

---

## Theme Activation

Pages select a theme via `themeKey` in the admin page builder. At render time:

```
Page.themeKey → useTheme()            → injects CSS + sets data-theme attr
             → useTemplateResolver()  → checks themeTemplateMap first, falls back to base templates
```

If a page has no `themeKey`, no theme CSS is injected and the base template is used.

---

## Key Files Reference

| File | Purpose |
|------|---------|
| `src/frontend/public/composables/useTheme.ts` | Fetches theme, injects CSS link and `data-theme` attribute |
| `src/frontend/public/composables/useTemplateResolver.ts` | Resolves template component with theme override support |
| `src/frontend/public/types/theme.ts` | Theme TypeScript interface |
| `src/backend/Seems.Domain/Entities/Theme.cs` | Theme entity (key, name, cssUrl) |
| `src/backend/Seems.Application/Themes/` | CQRS commands and queries for theme CRUD |

---

## Practical Checklist

- [ ] Create theme record in admin (key + cssUrl)
- [ ] Build and deploy CSS bundle to a public URL
- [ ] Scope all CSS to `[data-theme="your-key"]`
- [ ] Use CSS custom properties for design tokens
- [ ] (Optional) Create theme-specific template components
- [ ] (Optional) Register template overrides in `useTemplateResolver.ts`
- [ ] Assign the theme to a page via the admin page builder
