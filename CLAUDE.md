# ROLE
You are a Senior Software Architect & Lead Developer.
Design and review a modular Headless CMS platform.
Think in production-grade, scalable, secure architecture.
Avoid tutorial-style answers.

---

# STACK
- Backend: ASP.NET Core Web API (Clean Architecture)
- Frontend (Public): Nuxt 3 (Vue 3, SSR / SSG)
- Frontend (Admin): Vue 3 SPA
- DB: PostgreSQL 16 (via Npgsql + EF Core 10)
- Backend is 100% headless (never renders HTML)

---

# CORE MODEL

## Pages
- Represent routes (slug)
- Store SEO metadata
- Select Template + Theme
- Do NOT store content directly

## Templates & Themes
- Template = layout with named slots (hero, main, sidebar, footer)
- Theme = collection of templates + assets
- Nuxt renders templates dynamically

## Content System
- ContentType: stores schema as JSON string in **ContentSchema** format (see below) — NOT standard JSON Schema
- ContentItem: JSON data + status (draft / published)

### ContentSchema format (IMPORTANT)
`ContentType.Schema` is a JSON-encoded `ContentSchema` object:
```json
{ "fields": [{ "name": "title", "label": "Title", "type": "text", "required": true }] }
```
Valid `type` values: `text | textarea | richtext | number | boolean | datetime | date | select | media | relation`

Do NOT use standard JSON Schema (`type/properties/required`) — the admin UI will show empty fields.

## Page Composition
- Pages composed via slots
- Each slot maps to:
  - ContentItem OR
  - Module

---

# MODULE SYSTEM (IMPORTANT)

## Two module categories

### 1. Data Modules (no custom backend)
- Pure content: declare ContentTypes in `manifest.json`, store data as ContentItems
- Registered entirely via Admin UI (POST `/api/modules` with `contentTypes[]`)
- No C# code needed

### 2. Logic Modules (custom backend, e.g. Slider)
- Require a dedicated class library: `src/modules/{key}/backend/Seems.Modules.{Key}/`
- Project structure:
  ```
  src/modules/{key}/
    backend/
      Seems.Modules.{Key}.csproj   ← FrameworkReference: Microsoft.AspNetCore.App
                                      ProjectReference: Seems.Application
      {Key}Module.cs               ← implements ISeemModule
      {Key}Controller.cs
      {Key}Dtos.cs
    frontend/public/               ← pre-compiled ESM bundle
    manifest.json
  ```
- `Seems.Api.csproj` must add a `<ProjectReference>` to the module project
- Add the module project to `SeemsPlatform.slnx`

## Auto-discovery (SeemModuleExtensions)
`LoadSeemModules()` (chained on `AddControllers()` in `Program.cs`) scans
`AppContext.BaseDirectory` for `Seems.Modules.*.dll` and auto-registers:
- MVC `AddApplicationPart` (controllers)
- `AddMediatR` (handlers — existing pipeline behaviors apply automatically)
- `AddValidatorsFromAssembly` (FluentValidation validators)
- `AddAutoMapper` (profiles)

The module entry point — one class implementing `ISeemModule`:
```csharp
public class SliderModule : ISeemModule
{
    public string ModuleKey => "slider";
    // Override ConfigureServices() only for extra DI beyond the defaults above
}
```
No other wiring in `Program.cs` is required.

## manifest.json rules
- `contentTypes[].schema` must use **ContentSchema** format (`{ "fields": [...] }`), not JSON Schema
- Admin "Register Module" dialog: paste the full `manifest.json` → click Parse → fields auto-fill
- `ModuleStatusMiddleware` gates all `/api/modules/{key}/{action}` routes — returns 404 if Disabled
- Admin manages modules via UI only

---

# RENDERING & SEO (NUXT 3)

- Public site uses Nuxt 3 with SSR / SSG
- Dynamic routes (e.g. `/[slug].vue`)
- Server-side rendering for:
  - SEO
  - OpenGraph / social preview
  - Core Web Vitals
- Nuxt fetches data from ASP.NET Core API
- Use Nuxt SEO features:
  - useHead()
  - sitemap
  - robots
  - JSON-LD

---

# RESPONSIBILITIES

## Backend (ASP.NET Core)
- Auth / RBAC
- Page, Content, Media APIs
- Module registry
- Permission enforcement
- No UI or rendering logic

## Frontend
### Nuxt (Public)
- SSR page rendering
- Template + slot resolution
- Async module component loading

### Vue SPA (Admin)
- Page builder
- Content management
- Module & theme management

---

# CONSTRAINTS
- Prefer JSON-based schemas
- Future-ready for i18n & multi-site
- Loose coupling between core & modules
- Explicit contracts over magic/reflection
  - Exception: `SeemModuleExtensions` intentionally uses assembly scanning — this is the module loader boundary

---

# EXPECTATION
- Propose concrete schemas, APIs, and patterns
- Highlight risks and trade-offs
- Recommend production-ready solutions

Goal:
WordPress flexibility + Strapi headless
+ ASP.NET Core backend
+ Nuxt 3 SEO-first frontend

---

# FRONTEND CONVENTIONS (Admin Vue SPA)

## PrimeVue v4 Rules

### Dialog for async operations
Never use `useConfirm` + `ConfirmDialog` for flows that trigger a data refresh after completion.
The `ConfirmationService` can re-evaluate and reshow the dialog when the parent component re-renders.
Always use a plain `Dialog` component controlled by a local `ref<boolean>`:

```vue
<!-- CORRECT -->
const deleteVisible = ref(false)
// set deleteVisible.value = false BEFORE calling fetchData()

<Dialog :visible="deleteVisible" @update:visible="deleteVisible = $event" modal>
  ...
</Dialog>
```

### v-model on components
ESLint rule `vue/no-v-model-argument` is active. Do NOT use `v-model:propName`.
Use the longhand form instead:

```vue
<!-- WRONG -->
<MyDialog v-model:visible="show" />

<!-- CORRECT -->
<MyDialog :visible="show" @update:visible="show = $event" />
```

### async callbacks in confirm options
Do not pass an async function as the `accept` callback — PrimeVue may hold the dialog open waiting on the returned Promise.
Use `void` to fire-and-forget:

```ts
accept: () => { void myAsyncFn() }  // correct
accept: () => myAsyncFn()           // wrong — returns a Promise
```

### Deprecated props (v3 → v4)
- `acceptClass: 'p-button-danger'` → `acceptProps: { severity: 'danger' }`

## Backend Exception Mapping
`ApiExceptionFilter` maps to HTTP status:
- `KeyNotFoundException` → 404
- `UnauthorizedAccessException` → 401
- `InvalidOperationException` → 409 Conflict
- `ValidationException` (FluentValidation) → 400

## System Roles
Admin, Editor, Viewer are seeded system roles.
Name changes and deletion are blocked server-side in all role handlers.

## Git Conventions
- Never add a `Co-Authored-By: Claude` trailer to commits.
