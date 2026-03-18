# SEEMS Platform — Project Structure

## Overview

```
seems-platform/
├── CLAUDE.md                           # Architecture spec & AI instructions
├── .gitignore
├── .env.example                        # Environment variables template
├── docker-compose.yml                  # PostgreSQL 16 + pgAdmin 4
├── dev-up.sh / dev-up.ps1 / dev-up.cmd     # Start dev environment (bash / PowerShell / cmd)
├── dev-down.sh / dev-down.ps1 / dev-down.cmd # Stop containers (bash / PowerShell / cmd)
├── docs/                               # Documentation
│   ├── project-structure.md            # This file
│   ├── module-development.md           # How to build a SEEMS module
│   ├── theme-development.md            # How to build a SEEMS theme
│   ├── content-search.md               # Content item search architecture
│   └── slot-rendering-roadmap.md       # Future slot rendering capabilities
├── src/
│   ├── backend/                        # ASP.NET Core 10 Web API (Clean Architecture)
│   └── frontend/
│       ├── admin/                      # Vue 3 SPA — Admin panel
│       └── public/                     # Nuxt 3 — Public site (SSR/SSG)
└── deploy/                             # Docker & infra configs (planned)
```

---

## Frontend — Admin (`src/frontend/admin/`)

Vue 3 SPA built with Vite. Manages pages, content, templates, themes, media, and modules.

**Stack:** Vue 3 + PrimeVue 4 (Aura) + Pinia + Vue Router + Axios + TypeScript

| Port | Package Manager | Build |
|------|----------------|-------|
| 3001 | pnpm | `pnpm dev` / `pnpm build` |

```
src/frontend/admin/
├── index.html                              # SPA entry point
├── package.json                            # @seems/admin
├── vite.config.ts                          # Alias @/, proxy /api → localhost:5000
├── tsconfig.json
├── tsconfig.app.json
├── tsconfig.node.json
├── env.d.ts
│
└── src/
    ├── main.ts                             # App bootstrap: PrimeVue, Router, Pinia, Toast, Confirm
    ├── App.vue                             # Root: Toast + ConfirmDialog + RouterView
    │
    ├── router/
    │   └── index.ts                        # All admin routes + auth guard
    │
    ├── stores/
    │   └── auth.ts                         # Pinia: login/logout, JWT token, user state
    │
    ├── composables/
    │   └── useApi.ts                       # Axios instance with Bearer interceptor + 401 redirect
    │
    ├── types/
    │   ├── api.ts                          # ApiResponse<T>, PaginatedResponse<T>, ApiError
    │   └── auth.ts                         # User, LoginRequest, LoginResponse
    │
    ├── components/
    │   └── layout/
    │       ├── AdminShell.vue              # Sidebar + Topbar + content area frame
    │       ├── Sidebar.vue                 # PrimeVue Menu: Dashboard, Content, Appearance, System
    │       └── Topbar.vue                  # User dropdown with logout
    │
    ├── views/
    │   ├── auth/
    │   │   └── LoginView.vue              # Login form with error handling
    │   ├── dashboard/
    │   │   └── DashboardView.vue          # Dashboard (stub)
    │   ├── pages/
    │   │   └── PageListView.vue           # Page management (stub)
    │   ├── content/
    │   │   ├── ContentTypeListView.vue    # Content type management (stub)
    │   │   └── ContentItemListView.vue    # Content item management (stub)
    │   ├── templates/
    │   │   └── TemplateListView.vue       # Template management (stub)
    │   ├── themes/
    │   │   └── ThemeManagerView.vue       # Theme management (stub)
    │   ├── media/
    │   │   └── MediaLibraryView.vue       # Media library (stub)
    │   ├── modules/
    │   │   └── ModuleListView.vue         # Module management (stub)
    │   └── settings/
    │       └── SiteSettingsView.vue       # Site settings (stub)
    │
    └── assets/
        └── css/
            └── main.css                   # Base reset + typography
```

### Admin Routes

| Path | View | Auth |
|------|------|------|
| `/login` | LoginView | Public |
| `/` | DashboardView | Protected |
| `/pages` | PageListView | Protected |
| `/content-types` | ContentTypeListView | Protected |
| `/content-items` | ContentItemListView | Protected |
| `/templates` | TemplateListView | Protected |
| `/themes` | ThemeManagerView | Protected |
| `/media` | MediaLibraryView | Protected |
| `/modules` | ModuleListView | Protected |
| `/settings` | SiteSettingsView | Protected |

---

## Frontend — Public (`src/frontend/public/`)

Nuxt 3 SSR/SSG app. Renders CMS pages with SEO support, template-based layouts, and module integration.

**Stack:** Nuxt 3 + @nuxt/image + @nuxtjs/sitemap + @nuxtjs/robots + nuxt-schema-org + TypeScript

| Port | Package Manager | Build |
|------|----------------|-------|
| 3000 | pnpm | `pnpm dev` / `pnpm build` / `pnpm generate` |

```
src/frontend/public/
├── app.vue                                 # Root: NuxtLayout + NuxtPage
├── package.json                            # @seems/public
├── nuxt.config.ts                          # SSR, API proxy, modules, ISR route rules
├── tsconfig.json                           # Extends .nuxt/tsconfig.json
│
├── pages/
│   ├── index.vue                           # Homepage: resolves slug "/"
│   └── [...slug].vue                       # Catch-all: slug → page resolver → template
│
├── layouts/
│   ├── default.vue                         # SiteHeader + main + SiteFooter
│   └── blank.vue                           # No-chrome (landing pages)
│
├── components/
│   ├── slots/
│   │   ├── SlotRenderer.vue                # Dispatches to ContentRenderer or ModuleRenderer
│   │   └── SlotContainer.vue               # Wrapper div for slot content
│   │
│   ├── templates/
│   │   ├── TemplateRenderer.vue            # Resolves template key → component
│   │   ├── StandardPage.vue                # Hero + main/sidebar grid + footer
│   │   ├── LandingPage.vue                 # Full-width, no sidebar
│   │   └── BlogPost.vue                    # Article layout with title header
│   │
│   ├── content/
│   │   ├── ContentRenderer.vue             # Maps contentTypeKey → component
│   │   ├── RichText.vue                    # HTML content block (v-html)
│   │   ├── HeroBlock.vue                   # Hero with background image + CTA
│   │   └── ImageBlock.vue                  # NuxtImg with caption
│   │
│   ├── modules/
│   │   ├── ModuleRenderer.vue              # Suspense + defineAsyncComponent
│   │   └── ModuleFallback.vue              # Loading/error boundary
│   │
│   ├── seo/
│   │   ├── SeoHead.vue                     # useHead() + useServerSeoMeta() wrapper
│   │   └── JsonLd.vue                      # JSON-LD structured data injection
│   │
│   └── common/
│       ├── SiteHeader.vue                  # Logo + Navigation
│       ├── SiteFooter.vue                  # Copyright bar
│       └── Navigation.vue                  # Fetches nav tree from API
│
├── composables/
│   ├── usePageResolver.ts                  # GET /api/pages/by-slug/:slug
│   ├── useContentItem.ts                   # GET /api/content-items/:id
│   ├── useModuleLoader.ts                  # defineAsyncComponent from module registry
│   ├── useTemplateResolver.ts              # Template key → Vue component map
│   ├── useSeo.ts                           # useHead + useServerSeoMeta from SeoMeta
│   └── useNavigation.ts                    # GET /api/navigation
│
├── types/
│   ├── page.ts                             # Page, SlotMapping, SeoMeta
│   ├── content.ts                          # ContentType, ContentItem, ContentStatus
│   ├── template.ts                         # Template, TemplateSlotDef
│   └── module.ts                           # ModuleManifest, ModuleComponent
│
├── utils/
│   ├── api.ts                              # $fetch.create with runtimeConfig base URL
│   ├── content-type-map.ts                 # Content type key → component registry
│   └── module-registry.ts                  # Module key → async component loader
│
├── middleware/
│   └── preview.global.ts                   # ?preview=token → cookie for draft mode
│
├── server/
│   ├── api/__sitemap__/
│   │   └── urls.ts                         # Dynamic sitemap from backend pages API
│   └── middleware/
│       └── cache.ts                        # CDN cache headers (s-maxage + SWR)
│
└── assets/
    └── css/
        └── main.css                        # Base reset + typography
```

### Rendering Pipeline

```
Browser: GET /about
  │
  ▼
pages/[...slug].vue
  │
  ├─ usePageResolver("about")
  │    └─ GET /api/pages/by-slug/about
  │       → { page, templateKey, slots[], seo }
  │
  ├─ useSeo(page.seo)
  │    └─ useHead() + useServerSeoMeta()   ← SSR meta tags
  │
  └─ useTemplateResolver(page.templateKey)
       └─ StandardPage.vue
            │
            ├─ SlotRenderer (hero)
            │    └─ ContentRenderer → HeroBlock.vue
            │
            ├─ SlotRenderer (main)
            │    └─ ContentRenderer → RichText.vue
            │
            ├─ SlotRenderer (sidebar)
            │    └─ ModuleRenderer → defineAsyncComponent(...)
            │
            └─ SlotRenderer (footer)
                 └─ ContentRenderer → ...
```

---

## Backend — API (`src/backend/`)

ASP.NET Core 10 Web API with Clean Architecture, CQRS (MediatR), PostgreSQL.

**Stack:** .NET 10 + EF Core 10 + ASP.NET Identity + MediatR + FluentValidation + AutoMapper + PostgreSQL 16

| Port | Solution | Build |
|------|----------|-------|
| 5000 | SeemsPlatform.slnx | `dotnet build` / `dotnet run --project Seems.Api` |

```
src/backend/
├── SeemsPlatform.slnx
│
├── Seems.Domain/                           # Zero dependencies
│   ├── Common/
│   │   ├── BaseEntity.cs                   # Id (Guid), CreatedAt, UpdatedAt
│   │   └── IAuditableEntity.cs
│   ├── Entities/
│   │   ├── Page.cs                         # Slug, Path, Title, TemplateKey, SeoMeta (JSON), ShowInNavigation, IsDefault
│   │   ├── SlotMapping.cs                  # PageId, SlotKey, TargetType, TargetId, Order, Parameters (jsonb)
│   │   ├── ContentType.cs                  # Key, Name, Schema (JSON — ContentSchema format)
│   │   ├── ContentItem.cs                  # ContentTypeKey, Data (JSON), Status
│   │   ├── Template.cs                     # Key, Name, ThemeKey, Slots (JSON)
│   │   ├── Theme.cs                        # Key, Name
│   │   ├── Media.cs                        # FileName, Url, MimeType, Size
│   │   ├── Module.cs                       # ModuleKey, Name, Version, Status, DefaultParametersJson (jsonb)
│   │   └── Identity/
│   │       ├── AppUser.cs                  # extends IdentityUser<Guid>
│   │       └── AppRole.cs                  # extends IdentityRole<Guid>
│   ├── Enums/
│   │   ├── ContentStatus.cs               # Draft, Published, Archived
│   │   ├── SlotTargetType.cs              # Content, Module
│   │   └── ModuleStatus.cs               # Installed, Disabled
│   ├── ValueObjects/
│   │   └── SeoMeta.cs                     # Title, Description, Og*, Canonical, Robots
│   └── Interfaces/
│       ├── IRepository.cs                 # Generic CRUD
│       ├── IPageRepository.cs             # GetBySlug, GetPublishedPages, ResolveByPathAsync, hierarchy queries
│       ├── IContentRepository.cs          # GetByContentTypeKey, search
│       └── IUnitOfWork.cs
│
├── Seems.Application/                      # → Domain
│   ├── DependencyInjection.cs             # AddApplication(): MediatR, Validation, AutoMapper
│   ├── Common/
│   │   ├── Interfaces/
│   │   │   ├── ICurrentUser.cs
│   │   │   └── IJwtTokenService.cs
│   │   ├── Behaviors/
│   │   │   ├── ValidationBehavior.cs      # FluentValidation pipeline
│   │   │   └── LoggingBehavior.cs
│   │   ├── Models/
│   │   │   ├── Result.cs                  # Success/Failure wrapper
│   │   │   └── PaginatedList.cs
│   │   └── Mappings/
│   │       └── MappingProfile.cs
│   ├── Pages/
│   │   ├── Dtos/PageDto.cs                # PageDto, SlotMappingDto (with Parameters)
│   │   ├── Commands/
│   │   │   ├── CreatePage/                # Command + Handler + Validator
│   │   │   ├── UpdatePage/                # Command + Handler + Validator
│   │   │   ├── DeletePage/                # Command + Handler
│   │   │   ├── UpdatePageStatus/          # Command + Handler
│   │   │   ├── SetDefaultPage/            # Command + Handler
│   │   │   ├── ReorderPages/              # Command + Handler
│   │   │   ├── AddPageSlot/               # Command + Handler + Validator
│   │   │   ├── RemovePageSlot/            # Command + Handler
│   │   │   ├── ReorderPageSlots/          # Command + Handler
│   │   │   └── UpdateSlotParameters/      # Command + Handler
│   │   └── Queries/
│   │       ├── GetPageBySlug/             # Query + Handler (resolves by path, supports parametric slugs)
│   │       ├── GetPageById/               # Query + Handler
│   │       ├── GetDefaultPage/            # Query + Handler
│   │       ├── GetPageTree/               # Query + Handler (full tree for admin)
│   │       ├── GetNavigationPages/        # Query + Handler (published nav tree, ShowInNavigation filter)
│   │       └── ListPages/                 # Query + Handler (paginated)
│   ├── Content/
│   │   ├── Dtos/ContentTypeDto.cs, ContentItemDto.cs
│   │   ├── Commands/CreateContentItem/    # Command + Handler + Validator
│   │   └── Queries/
│   │       ├── GetContentItem/            # Query + Handler
│   │       └── ListContentItems/          # Query + Handler
│   ├── Templates/
│   │   ├── Dtos/TemplateDto.cs
│   │   └── Queries/ListTemplates/         # Query + Handler
│   ├── Media/
│   │   └── Dtos/MediaDto.cs
│   └── Identity/
│       ├── Dtos/LoginRequest.cs, LoginResponse.cs, UserDto.cs
│       └── Commands/Login/                # Command + Handler + Validator
│
├── Seems.Infrastructure/                   # → Application, Domain
│   ├── DependencyInjection.cs             # AddInfrastructure(): DbContext, Identity, JWT, repos
│   ├── Persistence/
│   │   ├── AppDbContext.cs                # IdentityDbContext<AppUser, AppRole, Guid>
│   │   ├── Configurations/               # EF Core fluent configs (8 files)
│   │   ├── Migrations/                   # EF Core migrations (InitialCreate)
│   │   ├── Repositories/
│   │   │   ├── Repository.cs             # Generic implementation
│   │   │   ├── PageRepository.cs         # Include Slots, filter by slug
│   │   │   └── ContentRepository.cs
│   │   ├── Seed/
│   │   │   └── DataSeeder.cs             # Roles, admin user, themes, templates, content types, sample data
│   │   └── UnitOfWork.cs
│   └── Identity/
│       ├── JwtTokenService.cs            # JWT generation with roles
│       └── CurrentUser.cs                # Claims-based current user
│
├── Seems.Api/                              # → Application, Infrastructure
│   ├── Program.cs                         # DI, JWT auth, Swagger, CORS, auto-migrate & seed (dev)
│   ├── appsettings.json                   # ConnectionStrings, JWT config, CORS origins
│   ├── appsettings.Development.json
│   ├── Controllers/
│   │   ├── AuthController.cs              # POST /api/auth/login
│   │   ├── PagesController.cs             # Full CRUD + by-slug + tree + sitemap + slots + slot parameters
│   │   ├── ContentTypesController.cs      # CRUD
│   │   ├── ContentItemsController.cs      # CRUD
│   │   ├── TemplatesController.cs         # CRUD
│   │   ├── ThemesController.cs            # CRUD
│   │   ├── MediaController.cs             # Upload, delete, folders CRUD
│   │   ├── ModulesController.cs           # CRUD + toggle status + /installed (public)
│   │   ├── NavigationController.cs        # GET /api/navigation → published nav tree (ISender)
│   │   ├── RolesController.cs             # CRUD (system roles immutable)
│   │   ├── UsersController.cs             # CRUD + reset-password + lock/unlock
│   │   ├── SettingsController.cs          # GET/PUT storage settings + GET/PUT site info
│   │   ├── CategoriesController.cs        # CRUD + tree
│   │   └── AuditLogsController.cs         # GET (read-only)
│   └── Filters/
│       └── ApiExceptionFilter.cs          # Validation, 401, 404, 409, 500 handling
│
└── Seems.Shared/                           # Zero dependencies (module contracts)
    └── Contracts/
        ├── IModuleManifest.cs
        └── SlotDescriptor.cs
```

### Dependency Graph

```
Seems.Domain        ← (no dependencies)
Seems.Shared        ← (no dependencies)
Seems.Application   ← Seems.Domain
Seems.Infrastructure ← Seems.Application, Seems.Domain
Seems.Api           ← Seems.Application, Seems.Infrastructure
```

### API Endpoints

#### Auth
| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| POST | `/api/auth/login` | Public | Authenticate, return JWT |

#### Pages
| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET | `/api/pages` | JWT | List pages (paginated) |
| GET | `/api/pages/{id}` | JWT | Get page by ID |
| GET | `/api/pages/default` | Public | Get the default (home) page |
| GET | `/api/pages/by-slug/{*slug}` | Public | Resolve page by path (exact + parametric) |
| GET | `/api/pages/tree` | JWT | Full page tree for admin |
| GET | `/api/pages/sitemap` | Public | Sitemap data |
| POST | `/api/pages` | JWT | Create page |
| PUT | `/api/pages/{id}` | JWT | Update page |
| DELETE | `/api/pages/{id}` | JWT | Delete page |
| PATCH | `/api/pages/{id}/status` | JWT | Publish / draft / archive |
| PATCH | `/api/pages/{id}/set-default` | JWT | Set as homepage |
| PATCH | `/api/pages/reorder` | JWT | Reorder pages (sort order) |
| POST | `/api/pages/{pageId}/slots` | JWT | Add slot mapping |
| DELETE | `/api/pages/{pageId}/slots/{slotId}` | JWT | Remove slot mapping |
| PATCH | `/api/pages/{pageId}/slots/{slotId}/parameters` | JWT | Update slot parameters (jsonb) |
| PATCH | `/api/pages/{pageId}/slots/order` | JWT | Reorder slot mappings |

#### Navigation
| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET | `/api/navigation` | Public | Published nav tree (ShowInNavigation, hierarchical) |

#### Content Types
| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET | `/api/content-types` | JWT | List content types |
| GET | `/api/content-types/{id}` | JWT | Get content type |
| POST | `/api/content-types` | JWT | Create content type |
| PUT | `/api/content-types/{id}` | JWT | Update content type |
| DELETE | `/api/content-types/{id}` | JWT | Delete content type |

#### Content Items
| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET | `/api/content-items` | JWT | List content items (paginated, filterable) |
| GET | `/api/content-items/{id}` | Public | Fetch content item |
| POST | `/api/content-items` | JWT | Create content item |
| PUT | `/api/content-items/{id}` | JWT | Update content item |
| DELETE | `/api/content-items/{id}` | JWT | Delete content item |

#### Templates & Themes
| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET | `/api/templates` | JWT | List templates |
| GET | `/api/templates/{id}` | JWT | Get template |
| POST | `/api/templates` | JWT | Create template |
| PUT | `/api/templates/{id}` | JWT | Update template |
| DELETE | `/api/templates/{id}` | JWT | Delete template |
| GET | `/api/themes` | JWT | List themes |
| GET | `/api/themes/{id}` | JWT | Get theme |
| GET | `/api/themes/by-key/{key}` | Public | Get active theme by key |
| POST | `/api/themes` | JWT | Create theme |
| PUT | `/api/themes/{id}` | JWT | Update theme |
| DELETE | `/api/themes/{id}` | JWT | Delete theme |

#### Media
| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET | `/api/media` | JWT | List media (paginated) |
| GET | `/api/media/{id}` | JWT | Get media item |
| POST | `/api/media/upload` | JWT | Upload file |
| DELETE | `/api/media/{id}` | JWT | Delete media |
| PATCH | `/api/media/{id}/move` | JWT | Move to folder |
| GET | `/api/media/folders` | JWT | List folders |
| POST | `/api/media/folders` | JWT | Create folder |
| PUT | `/api/media/folders/{id}` | JWT | Rename folder |
| DELETE | `/api/media/folders/{id}` | JWT | Delete folder |

#### Modules
| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET | `/api/modules` | JWT | List all modules |
| GET | `/api/modules/installed` | Public | List installed (active) modules |
| POST | `/api/modules` | JWT | Register module |
| PUT | `/api/modules/{id}` | JWT | Update module metadata |
| PATCH | `/api/modules/{id}` | JWT | Toggle Installed ↔ Disabled |
| DELETE | `/api/modules/{id}` | JWT | Unregister module |

#### Users, Roles & Settings
| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET/POST/PUT/DELETE | `/api/users` | JWT | User management (Admin only) |
| POST | `/api/users/{id}/reset-password` | JWT | Reset password |
| POST | `/api/users/{id}/lock` | JWT | Lock user account |
| POST | `/api/users/{id}/unlock` | JWT | Unlock user account |
| GET/POST/PUT/DELETE | `/api/roles` | JWT | Role management (system roles immutable) |
| GET/PUT | `/api/settings/site` | JWT | Site info (name, tagline, logo, favicon) |
| GET/PUT | `/api/settings/storage` | JWT | Storage provider configuration |
| GET | `/api/categories` | JWT | List categories |
| GET | `/api/categories/tree` | JWT | Category tree |
| GET | `/api/audit-logs` | JWT | Audit log viewer |

---

## Key Architecture Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Backend framework | ASP.NET Core 10 | Long-term support, high performance |
| Architecture | Clean Architecture + CQRS | Separation of concerns, testable |
| Database | PostgreSQL 16 (Npgsql.EntityFrameworkCore.PostgreSQL) | Native jsonb, EF Core 10 support, first-class JSON columns |
| Auth | ASP.NET Identity + JWT | Built-in user/role management |
| CQRS | MediatR | Pipeline behaviors for validation/logging |
| Validation | FluentValidation | Declarative, pipeline-integrated |
| Primary keys | Guid | Distributed-friendly, no auto-increment |
| JSON columns | EF Core owned types + JSON | SeoMeta, Schema, Data, Slots |
| Admin API client | Axios | Request/response interceptors for JWT + 401 handling |
| Public API client | Nuxt `$fetch` | SSR-safe, no extra dependency |
| Admin state | Pinia | Vue ecosystem standard |
| Admin UI | PrimeVue 4 (Aura) | Rich component set for admin dashboards |
| Public SEO | useHead + useServerSeoMeta | Server-side meta rendering for crawlers |
| Template system | Static component map | Explicit, no runtime magic, tree-shakeable |
| Module loading | defineAsyncComponent | Code-split, loaded on demand |
| Caching | ISR (60s) + SWR headers | Balance between freshness and performance |
| Preview mode | Cookie-based | Works across navigations, no URL pollution |

---

## Dev Quick Start

### One-command setup

**Linux / macOS / Git Bash:**
```bash
./dev-up.sh     # Starts PostgreSQL, pgAdmin, and ASP.NET Core API
./dev-down.sh   # Stops all containers
```

**Windows (PowerShell):**
```powershell
.\dev-up.ps1    # Starts PostgreSQL, pgAdmin, and ASP.NET Core API
.\dev-down.ps1  # Stops all containers
```

**Windows (cmd):**
```cmd
dev-up.cmd      # Starts PostgreSQL, pgAdmin, and ASP.NET Core API
dev-down.cmd    # Stops all containers
```

### Manual setup

```bash
# 1. Infrastructure (PostgreSQL + pgAdmin)
cp .env.example .env
docker compose up -d

# 2. Backend (port 5001) — auto-migrates & seeds in dev
cd src/backend
dotnet run --project Seems.Api
# Swagger: https://localhost:5001/swagger

# 3. Admin (port 3001)
cd src/frontend/admin
pnpm install
pnpm dev

# 4. Public (port 3000)
cd src/frontend/public
pnpm install
pnpm dev
```

### Dev Services

| Service | URL | Credentials |
|---------|-----|-------------|
| API / Swagger | https://localhost:5001/swagger | — |
| pgAdmin | http://localhost:8080 | admin@seems.local / admin |
| Admin SPA | http://localhost:3001 | admin@seems.local / Admin@123 |
| Public Site | http://localhost:3000 | — |

### Seed Data

On first run in development, the API auto-migrates the database and seeds:
- **Roles:** Admin, Editor, Viewer
- **Admin user:** admin@seems.local / Admin@123
- **Theme:** default
- **Templates:** standard-page, landing-page, blog-post
- **Content types:** rich-text, hero-block, image-block
- **Sample content:** Hero block + Rich text
- **Homepage:** `/` with hero and main slots mapped

Both frontends proxy `/api` to the backend.
