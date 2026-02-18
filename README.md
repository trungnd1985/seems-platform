# SEEMS Platform

A modular headless CMS that combines **WordPress flexibility** with **Strapi's headless approach**, powered by **ASP.NET Core** and **Nuxt 3**.

## Architecture

```
                    +-----------------------+
                    |     Public Site        |
                    |   Nuxt 3 (SSR/SSG)    |
                    |     Port 3000         |
                    +-----------+-----------+
                                |
                     GET /api/pages/by-slug/*
                                |
                    +-----------+-----------+
                    |    ASP.NET Core API    |
                    |  Clean Architecture    |
                    |   Port 5001 (HTTPS)    |
                    +-----------+-----------+
                                |
                    +-----------+-----------+
                    |    PostgreSQL 16       |
                    |     Port 5432         |
                    +-----------------------+
                                |
                    +-----------+-----------+
                    |     Admin Panel        |
                    |    Vue 3 SPA          |
                    |     Port 3001         |
                    +-----------------------+
```

| Layer | Stack |
|-------|-------|
| **Backend** | .NET 10, EF Core 10, ASP.NET Identity, MediatR, FluentValidation, PostgreSQL 16 |
| **Public Site** | Nuxt 3, SSR/SSG, SEO-first (`useHead`, sitemap, robots, JSON-LD) |
| **Admin Panel** | Vue 3, PrimeVue 4 (Aura), Pinia, Axios |
| **Database** | PostgreSQL 16 with native `jsonb` columns |
| **Infrastructure** | Docker Compose, pgAdmin 4 |

## Core Concepts

**Pages** are routes (slugs) that select a Template and Theme but store no content directly.

**Templates** define layouts with named slots (`hero`, `main`, `sidebar`, `footer`).

**Slots** are filled by either a **ContentItem** (JSON data validated against a ContentType schema) or a **Module** (installable package with its own APIs and Vue components).

**Content Types** are JSON schemas stored in the database — no migration needed when adding new types.

**Modules** are sandboxed, declarative packages that extend the platform. See [Module Development Guide](docs/module-development.md).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) with [pnpm](https://pnpm.io/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

## Quick Start

### One command

```bash
# Linux / macOS / Git Bash
./dev-up.sh

# Windows (PowerShell)
.\dev-up.ps1

# Windows (cmd)
dev-up.cmd
```

This starts PostgreSQL, pgAdmin, and the ASP.NET Core API with auto-migration and seed data.

### Manual

```bash
# 1. Infrastructure
cp .env.example .env        # or: copy .env.example .env (Windows)
docker compose up -d

# 2. Backend (auto-migrates & seeds in dev)
cd src/backend
dotnet run --project Seems.Api

# 3. Admin panel
cd src/frontend/admin
pnpm install && pnpm dev

# 4. Public site
cd src/frontend/public
pnpm install && pnpm dev
```

### Stop

```bash
./dev-down.sh               # Linux / macOS / Git Bash
.\dev-down.ps1              # Windows (PowerShell)
dev-down.cmd                # Windows (cmd)
```

## Dev Services

| Service | URL | Credentials |
|---------|-----|-------------|
| API (Swagger) | https://localhost:5001/swagger | -- |
| pgAdmin | http://localhost:8080 | admin@seems.local / admin |
| Admin Panel | http://localhost:3001 | admin@seems.local / Admin@123 |
| Public Site | http://localhost:3000 | -- |

## Seed Data

On first run, the API seeds the database with:

| Data | Values |
|------|--------|
| Roles | Admin, Editor, Viewer |
| Admin user | admin@seems.local / Admin@123 |
| Theme | `default` |
| Templates | `standard-page`, `landing-page`, `blog-post` |
| Content types | `rich-text`, `hero-block`, `image-block` |
| Sample content | Hero block + Rich text block |
| Homepage | `/` with hero and main slots |

## Project Structure

```
seems-platform/
├── docker-compose.yml          # PostgreSQL 16 + pgAdmin 4
├── .env.example                # Environment variables template
├── dev-up.sh / .ps1 / .cmd    # Start dev environment
├── dev-down.sh / .ps1 / .cmd  # Stop dev environment
├── docs/
│   ├── project-structure.md    # Full directory tree + architecture details
│   └── module-development.md   # Module development guide
└── src/
    ├── backend/                # ASP.NET Core 10 (Clean Architecture)
    │   ├── Seems.Domain/       # Entities, value objects, interfaces
    │   ├── Seems.Application/  # CQRS handlers, DTOs, validators
    │   ├── Seems.Infrastructure/ # EF Core, Identity, repositories
    │   ├── Seems.Api/          # Controllers, Program.cs
    │   └── Seems.Shared/       # Module contracts (IModuleManifest)
    └── frontend/
        ├── admin/              # Vue 3 SPA (PrimeVue 4)
        └── public/             # Nuxt 3 (SSR/SSG)
```

See [docs/project-structure.md](docs/project-structure.md) for the complete directory breakdown.

## API Endpoints

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| POST | `/api/auth/login` | Public | Authenticate, return JWT |
| GET | `/api/pages` | JWT | List pages (paginated) |
| GET | `/api/pages/by-slug/{slug}` | Public | Resolve page by slug |
| GET | `/api/pages/sitemap` | Public | Sitemap data |
| POST | `/api/pages` | JWT | Create page |
| GET | `/api/content-types` | JWT | List content types |
| POST | `/api/content-types` | JWT | Create content type |
| GET | `/api/content-items` | JWT | List items (paginated) |
| GET | `/api/content-items/{id}` | Public | Fetch content item |
| POST | `/api/content-items` | JWT | Create content item |
| GET | `/api/templates` | JWT | List templates |
| GET | `/api/themes` | JWT | List themes |
| GET | `/api/media` | JWT | List media |
| GET | `/api/modules` | JWT | List modules |
| GET | `/api/navigation` | Public | Site navigation tree |

## Rendering Pipeline

```
Browser: GET /about
  |
  v
pages/[...slug].vue
  |
  +-- usePageResolver("about")
  |     GET /api/pages/by-slug/about
  |     -> { page, templateKey, slots[], seo }
  |
  +-- useSeo(page.seo)
  |     useHead() + useServerSeoMeta()    <-- SSR meta tags
  |
  +-- useTemplateResolver(page.templateKey)
        StandardPage.vue
          |
          +-- SlotRenderer (hero)    -> ContentRenderer -> HeroBlock.vue
          +-- SlotRenderer (main)    -> ContentRenderer -> RichText.vue
          +-- SlotRenderer (sidebar) -> ModuleRenderer  -> async component
          +-- SlotRenderer (footer)  -> ContentRenderer -> ...
```

## Backend Architecture

Clean Architecture with CQRS (MediatR):

```
Seems.Domain          <- no dependencies
Seems.Shared          <- no dependencies (module contracts)
Seems.Application     <- Seems.Domain
Seems.Infrastructure  <- Seems.Application, Seems.Domain
Seems.Api             <- Seems.Application, Seems.Infrastructure
```

## Documentation

- [Project Structure](docs/project-structure.md) -- full directory tree, routes, architecture decisions
- [Module Development](docs/module-development.md) -- how to build and install modules

## License

Proprietary. All rights reserved.
