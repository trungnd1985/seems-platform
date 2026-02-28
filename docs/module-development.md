# Module Development Guide

## Overview

A SEEMS module is a **self-contained package** that extends the platform without modifying core code. Modules are declarative, sandboxed, and installable via the Admin UI.

A module can:

- Register backend API endpoints under `/api/modules/{moduleKey}/`
- Declare custom ContentTypes (JSON schemas)
- Provide a Vue 3 component for the **public site** (Nuxt 3, dynamically loaded ESM)
- Provide a Vue 3 settings page for the **admin panel** (Vue 3 SPA)
- Render inside any page template slot

A module **cannot**:

- Modify core database tables or EF Core migrations
- Access other modules' internals
- Override core API routes
- Import from `Seems.Domain` or `Seems.Infrastructure` directly (only `Seems.Application` is allowed)

---

## Two Module Categories

### 1. Data Modules (no C# code)

Pure content modules that only declare ContentTypes. Registered entirely via the Admin UI by pasting `manifest.json`. No C# project needed.

### 2. Logic Modules (custom backend, e.g. Slider)

Require a dedicated class library. The platform auto-discovers and registers them at startup.

```
src/modules/{key}/
├── manifest.json
├── backend/
│   └── Seems.Modules.{Key}/
│       ├── Seems.Modules.{Key}.csproj    ← FrameworkReference: Microsoft.AspNetCore.App
│       │                                    ProjectReference: Seems.Application
│       ├── {Key}Module.cs                ← implements ISeemModule
│       ├── {Key}Controller.cs
│       └── {Key}Dtos.cs
└── frontend/
    └── public/
        ├── {Key}.vue                     ← SSR-safe component (no Nuxt auto-imports)
        ├── vue-bridge.js                 ← Re-exports all Vue APIs from window.__SEEMS_VUE__
        ├── index.ts                      ← ESM entry (export { default } from './{Key}.vue')
        └── vite.config.ts                ← Library build (outputs to API wwwroot)
```

**Solution setup:**
- Add `<ProjectReference>` to `Seems.Modules.{Key}.csproj` in `Seems.Api.csproj`
- Add the module project to `SeemsPlatform.slnx`

---

## Step 1 — Create `manifest.json`

Every module must have a `manifest.json` at its root.

> **IMPORTANT:** `contentTypes[].schema` must use the platform **ContentSchema** format, not standard JSON Schema.

```json
{
  "moduleKey": "slider",
  "name": "Hero Slider",
  "version": "1.0.0",
  "description": "Full-width hero banner with autoplay",
  "author": "SEEMS Platform",
  "publicComponentUrl": "/modules/slider/component.js",
  "slots": [
    {
      "key": "slider",
      "label": "Hero Slider",
      "allowedTypes": null,
      "maxItems": 1
    }
  ],
  "contentTypes": [
    {
      "key": "slider-slide",
      "name": "Slider Slide",
      "schema": {
        "fields": [
          { "name": "title",    "label": "Title",       "type": "text",   "required": true  },
          { "name": "subtitle", "label": "Subtitle",    "type": "text",   "required": false },
          { "name": "imageUrl", "label": "Image URL",   "type": "media",  "required": true  },
          { "name": "ctaText",  "label": "CTA Text",    "type": "text",   "required": false },
          { "name": "ctaLink",  "label": "CTA Link",    "type": "text",   "required": false },
          { "name": "order",    "label": "Order",       "type": "number", "required": false }
        ]
      }
    }
  ],
  "apis": [
    { "method": "GET",    "path": "/api/modules/slider/slides" },
    { "method": "GET",    "path": "/api/modules/slider/slides/all" },
    { "method": "POST",   "path": "/api/modules/slider/slides" },
    { "method": "PUT",    "path": "/api/modules/slider/slides/{id}" },
    { "method": "DELETE", "path": "/api/modules/slider/slides/{id}" }
  ]
}
```

### ContentSchema format (CRITICAL)

`schema` must be a `ContentSchema` object — **not** standard JSON Schema:

```json
{ "fields": [{ "name": "title", "label": "Title", "type": "text", "required": true }] }
```

Valid `type` values: `text | textarea | richtext | number | boolean | datetime | date | select | media | relation`

Do **not** use `{ "type": "object", "properties": {...} }` — the admin UI will render empty fields.

### Manifest Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `moduleKey` | `string` | Yes | Unique kebab-case identifier. Immutable after registration. Used in URLs, DB, and component registry |
| `name` | `string` | Yes | Display name shown in Admin UI |
| `version` | `string` | Yes | SemVer string |
| `description` | `string` | No | Short description for Admin UI |
| `author` | `string` | No | Author name |
| `publicComponentUrl` | `string` | No | Relative path (`/modules/slider/component.js`) or HTTPS URL to the pre-built ESM bundle. Required for slot rendering |
| `slots` | `SlotDescriptor[]` | No | Slots this module can render into |
| `contentTypes` | `ContentTypeDecl[]` | No | Content types registered on install (ContentSchema format) |
| `apis` | `ApiRoute[]` | No | API routes exposed (documentation only, not enforced) |

---

## Step 2 — Backend: Module Entry Point + Controller

### ISeemModule (auto-discovery entry point)

```csharp
// Seems.Modules.Slider/SliderModule.cs
using Seems.Application.Modules;

namespace Seems.Modules.Slider;

public class SliderModule : ISeemModule
{
    public string ModuleKey => "slider";
    // Override ConfigureServices() only if you need extra DI beyond
    // what LoadSeemModules() registers automatically (controllers,
    // MediatR handlers, validators, AutoMapper profiles).
}
```

`LoadSeemModules()` (chained on `AddControllers()` in `Program.cs`) scans the output directory for `Seems.Modules.*.dll` and automatically:
- Calls `AddApplicationPart` (registers controllers)
- Calls `AddMediatR` (registers handlers — existing pipeline behaviors apply)
- Calls `AddValidatorsFromAssembly` (FluentValidation)
- Calls `AddAutoMapper` (profiles)

**No manual wiring in `Program.cs` is required.**

### Controller

```csharp
// Seems.Modules.Slider/SliderController.cs
[ApiController]
[Route("api/modules/slider")]
public class SliderController(ISender sender) : ControllerBase
{
    [HttpGet("slides")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublished(CancellationToken ct)
    {
        var items = await sender.Send(
            new ListContentItemsQuery("slider-slide", Status: "Published", PageSize: 100), ct);
        return Ok(items.Select(MapToSlideDto));
    }

    [HttpGet("slides/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await sender.Send(
            new ListContentItemsQuery("slider-slide", PageSize: 100), ct);
        return Ok(items.Select(MapToSlideDto));
    }

    [HttpPost("slides")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SlideDto>> Create([FromBody] CreateSlideRequest body, CancellationToken ct)
    {
        var item = await sender.Send(
            new CreateContentItemCommand("slider-slide", JsonSerializer.Serialize(body)), ct);
        return CreatedAtAction(nameof(GetPublished), MapToSlideDto(item));
    }

    [HttpPut("slides/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SlideDto>> Update(Guid id, [FromBody] UpdateSlideRequest body, CancellationToken ct)
    {
        var item = await sender.Send(
            new UpdateContentItemCommand(id, JsonSerializer.Serialize(body), body.Status), ct);
        return Ok(MapToSlideDto(item));
    }

    [HttpDelete("slides/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteContentItemCommand(id), ct);
        return NoContent();
    }

    private static SlideDto MapToSlideDto(ContentItemDto dto) { /* deserialize dto.Data */ }
}
```

### Backend Rules

- Route prefix **must** be `api/modules/{moduleKey}` (no trailing slash in attribute)
- Only reference `Seems.Application` — never `Seems.Infrastructure` directly
- Module API routes are gated by `ModuleStatusMiddleware`: returns 404 if the module is Disabled
- Use `[Authorize]` / `[AllowAnonymous]` per endpoint as appropriate
- Never modify core tables; use `ContentItem` JSON data for module-owned data

---

## Step 3 — Frontend: Public Component

The public component renders inside a page slot on the Nuxt 3 public site. It is loaded as a **standalone ESM bundle** — Nuxt auto-imports (`useFetch`, `useRuntimeConfig`, etc.) are **not available**.

### SSR Rules (standalone ESM — different from Nuxt pages)

| Rule | Reason |
|------|--------|
| No `useFetch` / `useAsyncData` | Nuxt auto-imports are compile-time only; unavailable in standalone bundles |
| Use plain `fetch()` inside `onMounted` | Browser-only; safe because module slots are client-rendered |
| No `window`, `document` at top level | The host Nuxt app runs SSR; top-level browser APIs break server rendering |
| Use `onMounted` / `onUnmounted` for timers and event listeners | Standard Vue lifecycle |
| Always receive `moduleKey: string` as a prop | Platform passes it when rendering the slot |

### Shared Vue Instance

A standalone ESM bundle must **not** bundle its own copy of Vue. Two Vue instances cause `currentInstance` isolation failures — `onMounted`/`onUnmounted` hooks silently fail to register.

The platform exposes the host Vue instance on `window.__SEEMS_VUE__` (set by `plugins/modules.client.ts` before any dynamic `import()`). Modules read from it via `vue-bridge.js`:

```js
// frontend/public/vue-bridge.js
const _v = window.__SEEMS_VUE__
export const ref              = _v.ref
export const computed         = _v.computed
export const watch            = _v.watch
export const onMounted        = _v.onMounted
export const onUnmounted      = _v.onUnmounted
export const defineProps      = _v.defineProps
export const openBlock        = _v.openBlock
export const createElementBlock = _v.createElementBlock
export const renderList       = _v.renderList
export const Fragment         = _v.Fragment
export const TransitionGroup  = _v.TransitionGroup
export const Transition       = _v.Transition
// add any other Vue APIs your component uses
export default _v
```

### Example Component

```vue
<!-- frontend/public/Slider.vue -->
<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'

const props = defineProps<{ moduleKey: string }>()

interface Slide {
  id: string
  title: string
  subtitle?: string
  imageUrl: string
  ctaText?: string
  ctaLink?: string
  order: number
}

const slides = ref<Slide[]>([])
const current = ref(0)
let timer: ReturnType<typeof setInterval> | null = null

function next() { current.value = (current.value + 1) % slides.value.length }
function prev() { current.value = (current.value - 1 + slides.value.length) % slides.value.length }

onMounted(async () => {
  try {
    const res = await fetch(`/api/modules/${props.moduleKey}/slides`)
    if (res.ok) slides.value = (await res.json()).sort((a: Slide, b: Slide) => a.order - b.order)
  } catch { /* non-fatal */ }
  if (slides.value.length > 1) timer = setInterval(next, 5000)
})

onUnmounted(() => { if (timer) clearInterval(timer) })
</script>
```

### Vite Config

```ts
// frontend/public/vite.config.ts
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import cssInjectedByJs from 'vite-plugin-css-injected-by-js'
import { fileURLToPath, URL } from 'node:url'

const vueBridgePath = fileURLToPath(new URL('./vue-bridge.js', import.meta.url))

export default defineConfig({
  plugins: [vue(), cssInjectedByJs()],
  resolve: {
    // Redirect bare 'vue' imports to the bridge file.
    // resolve.alias is applied by Vite core BEFORE plugin resolveId hooks,
    // which is why this works (a plugin-based approach does not).
    alias: { vue: vueBridgePath },
  },
  build: {
    // Output directly into the API's wwwroot so the API serves it at /modules/{key}/
    outDir: '../../../../backend/Seems.Api/wwwroot/modules/slider',
    emptyOutDir: true,
    lib: {
      entry: 'index.ts',
      formats: ['es'],
      fileName: 'component',
    },
  },
})
```

**Key points:**
- `resolve.alias: { vue: vueBridgePath }` — Rollup inlines `vue-bridge.js` so the output has zero `import from 'vue'` statements. The bundle reads from `window.__SEEMS_VUE__` at runtime.
- `cssInjectedByJs()` — embeds `<style>` injection in the JS so no separate `component.css` is needed. The bundle is fully self-contained.
- `outDir` points to the API project's `wwwroot/modules/{key}/` — the API serves the file at `/modules/{key}/component.js`.

### package.json

```json
{
  "name": "@seems-modules/slider-public",
  "type": "module",
  "scripts": {
    "build": "vite build",
    "dev": "vite build --watch"
  },
  "dependencies": { "vue": "^3.5.0" },
  "devDependencies": {
    "@types/node": "^22.0.0",
    "@vitejs/plugin-vue": "^5.2.0",
    "vite": "^6.0.0",
    "vite-plugin-css-injected-by-js": "^4.0.1"
  }
}
```

### index.ts

```ts
export { default } from './Slider.vue'
```

### Build

```bash
cd src/modules/slider/frontend/public
npm install
npm run build
# → Seems.Api/wwwroot/modules/slider/component.js (self-contained, ~7 KB)
```

---

## Step 4 — Nuxt Dev Proxy

In development, the browser's dynamic `import('/modules/slider/component.js')` hits the Nuxt dev server (port 3000). Add a Nitro devProxy so it forwards to the API's wwwroot:

```ts
// src/frontend/public/nuxt.config.ts
nitro: {
  devProxy: {
    '/modules': { target: 'http://localhost:5000/modules', changeOrigin: true },
  },
},
```

In production, configure your reverse proxy (nginx/Caddy) to forward `/modules/` to the API.

---

## Step 5 — Frontend: Admin Settings Page

Logic modules typically have a dedicated admin page for managing module-specific data (e.g. slides, form configuration).

### Route convention

Add a route named `module-{moduleKey}` inside the AdminShell children in `src/frontend/admin/src/router/index.ts`:

```ts
{
  path: 'modules/slider',
  name: 'module-slider',
  component: () => import('@/views/modules/slider/SliderSettingsView.vue'),
  meta: { roles: ['Admin'] },
},
```

### Configure button

The Module List automatically shows a gear icon for any module that has a matching named route (`module-{moduleKey}`). No additional wiring is needed — `hasSettingsPage()` in `ModuleListView.vue` resolves the route at runtime.

### Settings page pattern

Follow the pattern in `src/frontend/admin/src/views/modules/slider/SliderSettingsView.vue`:
- Inline composable using `useApi()` for module-specific endpoints
- DataTable with row actions: Edit (pencil), Publish/Unpublish toggle, Delete
- Delete: use a plain `Dialog` controlled by a local `ref<boolean>` — **not** `useConfirm` (see PrimeVue conventions)
- Form dialog: `:visible` + `@update:visible` — **not** `v-model:visible` (ESLint rule `vue/no-v-model-argument`)

---

## Step 6 — Register the Module

### Via Admin UI (recommended)

1. Navigate to **Admin → Modules → Register Module**
2. Paste the full `manifest.json` contents into the manifest textarea
3. Click **Parse** — all fields auto-fill, content types are listed
4. Click **Register**

### Via API

```http
POST /api/modules
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "moduleKey": "slider",
  "name": "Hero Slider",
  "version": "1.0.0",
  "publicComponentUrl": "/modules/slider/component.js",
  "description": "Full-width hero banner",
  "author": "SEEMS Platform",
  "contentTypes": [
    {
      "key": "slider-slide",
      "name": "Slider Slide",
      "schema": "{\"fields\":[{\"name\":\"title\",\"label\":\"Title\",\"type\":\"text\",\"required\":true}]}"
    }
  ]
}
```

`publicComponentUrl` accepts:
- **Relative path**: `/modules/slider/component.js` (served from the API's wwwroot)
- **Absolute HTTPS URL**: `https://cdn.example.com/modules/slider/component.js`

### Updating module metadata

```http
PUT /api/modules/{id}
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "name": "Hero Slider",
  "version": "1.1.0",
  "publicComponentUrl": "/modules/slider/component.js",
  "description": "Updated description",
  "author": "SEEMS Platform"
}
```

`moduleKey` is immutable — it is not part of the update payload. Use the pencil icon in the Admin Module List to edit metadata via the UI.

### Module Lifecycle

```
POST   /api/modules          → Status: Installed  (active, renders in slots)
PUT    /api/modules/:id      → Updates metadata (name, version, url, description, author)
PATCH  /api/modules/:id      → Toggles status: Installed ↔ Disabled
                               (Disabled: all /api/modules/{key}/* return 404)
DELETE /api/modules/:id      → Removed (ContentItem data is preserved)
```

### Automatic runtime loading

On every browser load the Nuxt plugin `plugins/modules.client.ts` automatically:

1. Exposes `window.__SEEMS_VUE__` (the host Vue instance) — must happen before any `import()`
2. Calls `GET /api/modules/installed` (public, no auth)
3. For each module with a `publicComponentUrl`, registers an async loader in the module registry

```ts
registerModuleComponent(mod.moduleKey, async () => {
  const remote = await import(/* @vite-ignore */ url)
  return remote.default ?? remote
})
```

No Nuxt rebuild is needed. New modules take effect on the next page load after registration.

---

## Step 7 — Assign to a Page Slot

1. Edit a page → select a slot (e.g., `hero`)
2. Set **Target Type: Module**
3. Search and select the module (e.g., `slider`)
4. Save

### Rendering pipeline

```
Page → Template → SlotRenderer
                    │
                    ├─ targetType === "content" → ContentRenderer
                    └─ targetType === "module"  → ModuleRenderer
                                                    │
                                                    ├─ useModuleLoader("slider")
                                                    ├─ module-registry lookup
                                                    ├─ defineAsyncComponent(loader)
                                                    └─ <Slider moduleKey="slider" />
```

> **Note:** Module slots render **client-side only** (the plugin runs in the browser). They are excluded from SSR HTML. Use `ContentItem` slots for SEO-critical content.

---

## Conventions & Constraints

| Rule | Rationale |
|------|-----------|
| `moduleKey` must be globally unique kebab-case | Used in URLs, DB, component registry, and slot mappings |
| `moduleKey` is immutable after registration | Changing it would break slot mappings and existing content |
| All backend routes must start with `api/modules/{moduleKey}` | Namespace isolation; prevents collision with core APIs |
| ContentType keys must be prefixed with `{moduleKey}-` | Avoids collisions (e.g., `slider-slide`, not `slide`) |
| `contentTypes[].schema` must use ContentSchema format | Standard JSON Schema causes empty fields in the admin UI |
| Public component must not bundle its own Vue | Two Vue instances break `onMounted`/`onUnmounted` via `currentInstance` isolation |
| Use `vue-bridge.js` + `resolve.alias` in vite.config | `external: ['vue']` produces bare `import from 'vue'` which browsers cannot resolve |
| Use `vite-plugin-css-injected-by-js` | Embeds styles in JS — no separate CSS file to load |
| Use `fetch()` in `onMounted` — not `useFetch` | `useFetch` is a Nuxt auto-import; unavailable in standalone ESM bundles |
| `publicComponentUrl` must be `/…` or `https://…` | Validator rejects other formats |
| Only reference `Seems.Application` from backend modules | Loose coupling; modules must not depend on `Seems.Infrastructure` internals |
| Module data goes through `ContentItem` or module-owned storage | No direct core table modifications allowed |

---

## TypeScript Types

```typescript
// src/frontend/admin/src/types/modules.ts

export interface Module {
  id: string
  moduleKey: string
  name: string
  version: string
  status: ModuleStatus
  publicComponentUrl?: string
  description?: string
  author?: string
  createdAt: string
  updatedAt: string
}

export type ModuleStatus = 'Installed' | 'Disabled'

export interface RegisterModuleRequest {
  moduleKey: string
  name: string
  version: string
  publicComponentUrl?: string
  description?: string
  author?: string
  contentTypes?: ContentTypeDecl[]
}

export interface UpdateModuleRequest {
  name: string
  version: string
  publicComponentUrl?: string
  description?: string
  author?: string
}

export interface ContentTypeDecl {
  key: string
  name: string
  schema: string // serialised ContentSchema JSON
}
```

```typescript
// src/frontend/public/types/module.ts

export interface InstalledModule {
  moduleKey: string
  name: string
  version: string
  publicComponentUrl?: string
}
```

---

## Troubleshooting

| Problem | Cause | Fix |
|---------|-------|-----|
| Module component not rendering | Registry lookup fails | Verify `moduleKey` in DB matches `targetId` in `SlotMapping` exactly |
| `Failed to resolve module specifier "vue"` | Module bundle uses `external: ['vue']` | Use `vue-bridge.js` + `resolve.alias: { vue: vueBridgePath }` in vite.config |
| `onMounted` / `onUnmounted` silently ignored | Two Vue instances (module bundled its own Vue) | Fix vite.config — bundle must read from `window.__SEEMS_VUE__` |
| Slot shows "Loading module..." forever | `import(url)` failed | Check browser console; verify CORS headers and that the file is valid ESM |
| Slot shows "Module could not be loaded" | `publicComponentUrl` is null | Set it via Admin Module List → pencil icon → edit |
| Module API returns 404 | Module is Disabled or route mismatch | Enable the module; check `[Route]` attribute matches `moduleKey` |
| CSS not applied | Styles in separate `component.css` not loaded | Add `vite-plugin-css-injected-by-js` to vite.config |
| `useFetch` is not defined | Using Nuxt auto-import in standalone ESM | Replace with `fetch()` inside `onMounted` |
| Content type not appearing in Admin | Install did not register content types | Re-register: DELETE + POST /api/modules with `contentTypes[]` in body |
| Configure gear icon not showing | No named route `module-{key}` registered | Add the route to `src/frontend/admin/src/router/index.ts` |
| Admin content type fields empty | Schema uses standard JSON Schema instead of ContentSchema | Change schema to `{ "fields": [{ "name", "label", "type", "required" }] }` |
