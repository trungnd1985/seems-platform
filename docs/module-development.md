# Module Development Guide

## Overview

A SEEMS module is a **self-contained package** that extends the platform without modifying core code. Modules are declarative, sandboxed, and installable via the Admin UI.

A module can:

- Register backend API endpoints under `/api/modules/{moduleKey}/`
- Declare custom ContentTypes (JSON schemas)
- Provide a Vue 3 component for the **public site** (Nuxt 3 SSR)
- Provide a Vue 3 component for the **admin panel** (Vue 3 SPA)
- Render inside any page template slot

A module **cannot**:

- Modify core database tables or EF Core migrations
- Access other modules' internals
- Override core API routes
- Import from `Seems.Domain` or `Seems.Infrastructure` directly

---

## Module Package Structure

```
seems-module-{moduleKey}/
├── manifest.json                     # Module metadata and declarations
├── backend/
│   ├── {ModuleName}Module.cs         # DI registration extension method
│   ├── Controllers/
│   │   └── {ModuleName}Controller.cs # Routes: /api/modules/{moduleKey}/*
│   ├── Models/                        # DTOs specific to this module
│   └── Services/
│       ├── I{ModuleName}Service.cs
│       └── {ModuleName}Service.cs
└── frontend/
    ├── public/
    │   ├── {ModuleName}.vue           # SSR-safe public component
    │   ├── index.ts                   # Vite library entry point
    │   └── vite.config.ts             # Builds to a single ESM bundle
    └── admin/
        └── {ModuleName}Settings.vue   # Admin configuration UI
```

---

## Step 1 — Create `manifest.json`

Every module must have a `manifest.json` at its root. This is the module's contract with the platform.

```json
{
  "moduleKey": "contact-form",
  "name": "Contact Form",
  "version": "1.0.0",
  "description": "Adds a contact form with email notifications",
  "author": "SEEMS Modules",
  "slots": [
    {
      "key": "contact-form",
      "label": "Contact Form",
      "allowedTypes": null,
      "maxItems": 1
    }
  ],
  "contentTypes": [
    {
      "key": "contact-form-config",
      "name": "Contact Form Config",
      "schema": {
        "type": "object",
        "properties": {
          "recipientEmail": { "type": "string", "format": "email" },
          "subject": { "type": "string" },
          "successMessage": { "type": "string" },
          "fields": {
            "type": "array",
            "items": {
              "type": "object",
              "properties": {
                "name": { "type": "string" },
                "label": { "type": "string" },
                "type": { "type": "string", "enum": ["text", "email", "textarea"] },
                "required": { "type": "boolean" }
              },
              "required": ["name", "label", "type"]
            }
          }
        },
        "required": ["recipientEmail", "fields"]
      }
    }
  ],
  "apis": [
    { "method": "POST", "path": "/api/modules/contact-form/submit" },
    { "method": "GET",  "path": "/api/modules/contact-form/config" },
    { "method": "PUT",  "path": "/api/modules/contact-form/config" }
  ]
}
```

### Manifest Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `moduleKey` | `string` | Yes | Unique kebab-case identifier. Used in URLs, DB, and component registry |
| `name` | `string` | Yes | Display name shown in Admin UI |
| `version` | `string` | Yes | SemVer string |
| `description` | `string` | No | Short description for Admin UI |
| `author` | `string` | No | Author name |
| `slots` | `SlotDescriptor[]` | No | Slots this module can render into |
| `contentTypes` | `ContentTypeDecl[]` | No | Content types registered on install |
| `apis` | `ApiRoute[]` | No | API routes exposed (documentation, not enforced) |

---

## Step 2 — Backend: Controller

Module APIs are namespaced under `/api/modules/{moduleKey}/`.

```csharp
// backend/Controllers/ContactFormController.cs
using Microsoft.AspNetCore.Mvc;

namespace Seems.Modules.ContactForm.Controllers;

[ApiController]
[Route("api/modules/contact-form")]
public class ContactFormController(IContactFormService service) : ControllerBase
{
    [HttpPost("submit")]
    [AllowAnonymous]
    public async Task<IActionResult> Submit([FromBody] ContactFormSubmission body)
    {
        await service.SubmitAsync(body);
        return Ok(new { message = "Submitted successfully" });
    }

    [HttpGet("config")]
    [AllowAnonymous]
    public async Task<IActionResult> GetConfig()
        => Ok(await service.GetConfigAsync());

    [HttpPut("config")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateConfig([FromBody] ContactFormConfig config)
    {
        await service.SaveConfigAsync(config);
        return Ok();
    }
}
```

### DI Registration

```csharp
// backend/ContactFormModule.cs
using Microsoft.Extensions.DependencyInjection;

namespace Seems.Modules.ContactForm;

public static class ContactFormModule
{
    public static IServiceCollection AddContactFormModule(this IServiceCollection services)
    {
        services.AddScoped<IContactFormService, ContactFormService>();
        return services;
    }
}
```

Call this from the host `Program.cs`:

```csharp
builder.Services.AddContactFormModule();
```

### Backend Rules

- Route prefix **must** be `/api/modules/{moduleKey}/`
- Only reference `Seems.Shared.Contracts` — never `Seems.Domain` or `Seems.Infrastructure`
- Use `[Authorize]` / `[AllowAnonymous]` as appropriate per endpoint
- Never modify core tables. Use module-owned storage or `ContentItem` JSON data

### Backend Contract Interface

Implement `IModuleManifest` from `Seems.Shared` if you need the manifest to be discoverable at runtime:

```csharp
using Seems.Shared.Contracts;

namespace Seems.Modules.ContactForm;

public class ContactFormManifest : IModuleManifest
{
    public string ModuleKey => "contact-form";
    public string Name => "Contact Form";
    public string Version => "1.0.0";
    public IReadOnlyList<SlotDescriptor> Slots =>
    [
        new("contact-form", "Contact Form", MaxItems: 1)
    ];
}
```

---

## Step 3 — Frontend: Public Component

The public component renders inside a page slot on the Nuxt 3 public site. It must be **SSR-safe**.

```vue
<!-- frontend/public/ContactForm.vue -->
<script setup lang="ts">
const props = defineProps<{ moduleKey: string }>()

const { data: config } = await useFetch(`/api/modules/${props.moduleKey}/config`)

const formData = ref<Record<string, string>>({})
const submitted = ref(false)
const error = ref<string | null>(null)

async function handleSubmit() {
  try {
    await $fetch(`/api/modules/${props.moduleKey}/submit`, {
      method: 'POST',
      body: formData.value,
    })
    submitted.value = true
  }
  catch {
    error.value = 'Submission failed. Please try again.'
  }
}
</script>

<template>
  <form v-if="config && !submitted" @submit.prevent="handleSubmit">
    <div v-for="field in (config.fields as any[])" :key="field.name">
      <label :for="field.name">{{ field.label }}</label>
      <textarea
        v-if="field.type === 'textarea'"
        :id="field.name"
        v-model="formData[field.name]"
        :required="field.required"
      />
      <input
        v-else
        :id="field.name"
        :type="field.type"
        v-model="formData[field.name]"
        :required="field.required"
      />
    </div>
    <button type="submit">Send</button>
  </form>
  <div v-else-if="submitted">
    <p>{{ (config as any)?.successMessage ?? 'Thank you!' }}</p>
  </div>
</template>
```

### SSR Rules

- No `window`, `document`, or browser-only APIs at top level
- Use `onMounted()` for browser-only logic
- Use `useFetch` or `useAsyncData` — never raw `fetch` — for data that needs SSR hydration
- The component always receives `moduleKey: string` as a prop

### Build as ESM Bundle

The public component must be compiled and bundled as a **pre-built ESM file** and hosted at a publicly accessible URL. The platform dynamically imports it at runtime — raw `.vue` files are not supported.

```ts
// frontend/public/vite.config.ts
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  build: {
    lib: {
      entry: 'index.ts',
      formats: ['es'],
      fileName: 'component',
    },
    rollupOptions: {
      // Vue is provided by the host Nuxt app — do not bundle it
      external: ['vue'],
      output: {
        globals: { vue: 'Vue' },
      },
    },
  },
})
```

```ts
// frontend/public/index.ts
export { default } from './ContactForm.vue'
```

Build output: `dist/component.js`

Upload `dist/component.js` to a CDN (or the platform's Media Library) to get the `publicComponentUrl`.

---

## Step 4 — Frontend: Admin Component

The admin component appears in the Admin SPA for module configuration.

```vue
<!-- frontend/admin/ContactFormSettings.vue -->
<script setup lang="ts">
import { useApi } from '@/composables/useApi'

const api = useApi()
const config = ref({ recipientEmail: '', subject: '', successMessage: '', fields: [] })

onMounted(async () => {
  config.value = await api.get('/api/modules/contact-form/config')
})

async function save() {
  await api.put('/api/modules/contact-form/config', config.value)
}
</script>

<template>
  <div>
    <h2>Contact Form Settings</h2>
    <InputText v-model="config.recipientEmail" placeholder="Recipient email" />
    <InputText v-model="config.subject" placeholder="Subject" />
    <InputText v-model="config.successMessage" placeholder="Success message" />
    <Button label="Save" @click="save" />
  </div>
</template>
```

---

## Step 5 — Register the Module

Registration has two parts: a one-time API call to store the module record, and the automatic runtime loading that happens on every page request.

### 5a — Insert via Admin API

```http
POST /api/modules
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "moduleKey": "contact-form",
  "name": "Contact Form",
  "version": "1.0.0",
  "publicComponentUrl": "https://cdn.example.com/modules/contact-form/1.0.0/component.js"
}
```

`publicComponentUrl` is the CDN URL of the ESM bundle built in Step 3. This is the only required registration step.

### 5b — Automatic runtime loading

On every browser load the Nuxt plugin `plugins/modules.client.ts` automatically:

1. Calls `GET /api/modules/installed` (public endpoint, no auth required)
2. For each module with a `publicComponentUrl`, registers an async loader:

```ts
registerModuleComponent(mod.moduleKey, async () => {
  const remote = await import(/* @vite-ignore */ mod.publicComponentUrl)
  return remote.default ?? remote
})
```

No Nuxt rebuild is needed. New modules take effect on the next page load after registration.

### Module Lifecycle

```
POST /api/modules          → Status: Installed  (active, renders in slots)
PATCH /api/modules/:id     → Status: Disabled   (APIs return 404, slots show fallback)
PATCH /api/modules/:id     → Status: Installed  (re-enabled)
DELETE /api/modules/:id    → Removed            (data optionally preserved in ContentItems)
```

---

## Step 6 — Assign to a Page Slot

Once installed, assign the module to a page slot in the Admin page builder:

1. Edit a page → select a slot (e.g., `sidebar`)
2. Set **Target Type: Module**
3. Select the module key (e.g., `contact-form`)
4. Save

This creates a `SlotMapping`:

```json
{
  "pageId": "...",
  "slotKey": "sidebar",
  "targetType": "Module",
  "targetId": "contact-form",
  "order": 0
}
```

### Rendering pipeline

```
Page → Template → SlotRenderer
                    │
                    ├─ targetType === "content" → ContentRenderer
                    └─ targetType === "module"  → ModuleRenderer
                                                    │
                                                    ├─ useModuleLoader("contact-form")
                                                    ├─ module-registry lookup
                                                    ├─ defineAsyncComponent(loader)
                                                    └─ <ContactForm moduleKey="contact-form" />
```

> **Note:** Module slots render **client-side only** (the plugin runs in the browser). They are not included in the SSR HTML. Use `ContentItem` slots for SEO-critical content.

---

## Conventions & Constraints

| Rule | Rationale |
|------|-----------|
| `moduleKey` must be globally unique kebab-case | Used in URLs, DB keys, component registry, and slot mappings |
| All backend routes must start with `/api/modules/{moduleKey}/` | Namespace isolation; prevents collision with core APIs |
| ContentType keys must be prefixed with `{moduleKey}-` | Avoids collisions (e.g., `contact-form-config`, not `config`) |
| Public component must be SSR-safe | Nuxt 3 renders server-side first |
| `publicComponentUrl` must serve a pre-compiled ESM with `default` export | The plugin does `remote.default ?? remote` — raw `.vue` files will not work |
| Only reference `Seems.Shared.Contracts` from backend module code | Loose coupling; modules must not depend on core internals |
| Schema changes use JSON — never EF Core migrations | Module content types are schema-driven, stored as JSON in `ContentTypes.Schema` |
| Module data goes through `ContentItem` or module-owned storage | No direct core table modifications |

---

## TypeScript Types

```typescript
// src/frontend/public/types/module.ts

export interface ModuleManifest {
  moduleKey: string
  name: string
  version: string
  contentTypes?: { key: string; schema: Record<string, unknown> }[]
  apis?: { method: string; path: string }[]
}

export interface InstalledModule {
  moduleKey: string
  name: string
  version: string
  publicComponentUrl?: string
}
```

---

## Backend Contract Types

```csharp
// Seems.Shared.Contracts.IModuleManifest
public interface IModuleManifest
{
    string ModuleKey { get; }
    string Name { get; }
    string Version { get; }
    IReadOnlyList<SlotDescriptor> Slots { get; }
}

// Seems.Shared.Contracts.SlotDescriptor
public record SlotDescriptor(
    string Key,
    string Label,
    string[]? AllowedTypes = null,
    int? MaxItems = null
);
```

---

## Troubleshooting

| Problem | Cause | Fix |
|---------|-------|-----|
| Module component not rendering | Plugin loaded but registry lookup fails | Verify `moduleKey` in DB matches `targetId` in `SlotMapping` exactly |
| Slot shows "Loading module..." forever | `import(url)` failed | Open browser console; check CORS headers on the CDN URL and that the file is valid ESM |
| Slot shows "Module could not be loaded" | `publicComponentUrl` is null or not set | Set `publicComponentUrl` on the module record via Admin API |
| Module API returns 404 | Route mismatch or module is Disabled | Check route attribute; verify `Module.Status = Installed` in DB |
| Content type not appearing in Admin | Module install did not register content types | Manually `POST /api/content-types` with the schema from the manifest |
| Module renders on client but not in SSR | Top-level browser API usage | Move `window`/`document` access inside `onMounted()` |
