# Module Development Guide

## Overview

A SEEMS module is a **self-contained package** that extends the platform without modifying core code. Modules are declarative, sandboxed, and installable via the Admin UI.

A module can:

- Register backend API endpoints under `/api/modules/{moduleKey}/`
- Declare custom ContentTypes (JSON schemas)
- Provide Vue components for the **public site** (Nuxt 3 SSR)
- Provide Vue components for the **admin panel** (Vue 3 SPA)
- Render inside page template slots

A module **cannot**:

- Modify core database tables or migrations
- Access other modules' internals
- Override core API routes
- Execute arbitrary server-side code outside its own API scope

---

## Module Structure

```
seems-module-{moduleKey}/
├── manifest.json              # Module metadata and declarations
├── backend/
│   ├── Controllers/           # API controllers
│   └── Services/              # Business logic
├── frontend/
│   ├── public/                # Nuxt components (SSR-safe)
│   │   ├── components/
│   │   │   └── {ModuleName}.vue
│   │   └── index.ts           # Public entry point — exports component loader
│   └── admin/                 # Admin panel components
│       ├── components/
│       │   └── {ModuleName}Settings.vue
│       └── index.ts           # Admin entry point
└── README.md
```

---

## Step 1: Create the Manifest

Every module must have a `manifest.json` at its root. This file is the module's contract with the platform.

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
    { "method": "GET", "path": "/api/modules/contact-form/submissions" }
  ]
}
```

### Manifest Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `moduleKey` | `string` | Yes | Unique identifier. Kebab-case, no spaces. Used in URLs, slot mappings, and component registry |
| `name` | `string` | Yes | Display name shown in Admin UI |
| `version` | `string` | Yes | SemVer version string |
| `description` | `string` | No | Short description for Admin UI |
| `author` | `string` | No | Author name |
| `slots` | `SlotDescriptor[]` | No | Slots this module can render into |
| `contentTypes` | `ContentTypeDecl[]` | No | Content types the module registers on install |
| `apis` | `ApiRoute[]` | No | API routes the module exposes (documentation only, enforced by backend registration) |

### Slot Descriptor

Maps to `Seems.Shared.Contracts.SlotDescriptor`:

```
{
  "key": "contact-form",      // Unique slot key
  "label": "Contact Form",    // Display label for Admin page builder
  "allowedTypes": null,        // Optional: restrict which content types can fill this slot
  "maxItems": 1                // Optional: max items in this slot (null = unlimited)
}
```

---

## Step 2: Backend — API Endpoints

Module APIs are namespaced under `/api/modules/{moduleKey}/`. The core platform routes requests to the module's controller.

### Controller Pattern

```csharp
using Microsoft.AspNetCore.Mvc;

namespace Seems.Modules.ContactForm.Controllers;

[ApiController]
[Route("api/modules/contact-form")]
public class ContactFormController : ControllerBase
{
    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] ContactFormSubmission submission)
    {
        // Validate against the module's content type schema
        // Process submission (send email, store record, etc.)
        return Ok(new { message = "Form submitted successfully" });
    }

    [HttpGet("submissions")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListSubmissions()
    {
        // Return submissions for admin review
        return Ok(new List<object>());
    }
}
```

### Rules

1. **Route prefix**: Always use `api/modules/{moduleKey}` — the platform enforces this namespace
2. **Auth**: Use `[Authorize]` or `[AllowAnonymous]` as needed. Public-facing endpoints (like form submit) may be anonymous; admin endpoints require auth
3. **No direct DbContext access**: Modules use the platform's `IRepository<>` and `IUnitOfWork` for data access, or bring their own isolated storage
4. **No core entity modification**: Modules must not alter core tables (Pages, ContentItems, etc.)

### Registering Module Services

Modules provide a DI registration extension method:

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace Seems.Modules.ContactForm;

public static class ContactFormModule
{
    public static IServiceCollection AddContactFormModule(this IServiceCollection services)
    {
        services.AddScoped<IContactFormService, ContactFormService>();
        // Register module-specific services
        return services;
    }
}
```

This is called during module installation, wired through the module registry in `Program.cs`.

---

## Step 3: Frontend — Public Component (Nuxt 3)

Public-facing module components render inside page template slots via SSR.

### Component

```vue
<!-- frontend/public/components/ContactForm.vue -->
<script setup lang="ts">
const props = defineProps<{
  moduleKey: string
}>()

const config = ref<Record<string, unknown> | null>(null)
const formData = ref<Record<string, string>>({})
const submitted = ref(false)
const error = ref<string | null>(null)

// Fetch module config from content item
const { data } = await useFetch(`/api/modules/${props.moduleKey}/config`)
config.value = data.value

async function handleSubmit() {
  try {
    await $fetch(`/api/modules/${props.moduleKey}/submit`, {
      method: 'POST',
      body: formData.value,
    })
    submitted.value = true
  } catch (e) {
    error.value = 'Failed to submit form. Please try again.'
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
    <p>{{ config?.successMessage || 'Thank you!' }}</p>
  </div>
</template>
```

### Entry Point — Registration

```typescript
// frontend/public/index.ts
import { registerModuleComponent } from '~/utils/module-registry'

registerModuleComponent('contact-form', () =>
  import('./components/ContactForm.vue')
)
```

### How It Works

The rendering pipeline for modules:

```
Page → Template → SlotRenderer
                    │
                    ├─ targetType === 'content' → ContentRenderer
                    └─ targetType === 'module'  → ModuleRenderer
                                                    │
                                                    ├─ useModuleLoader(moduleKey)
                                                    ├─ module-registry lookup
                                                    └─ defineAsyncComponent → ContactForm.vue
```

1. A page's `SlotMapping` with `targetType: Module` and `targetId: "contact-form"` triggers `ModuleRenderer`
2. `ModuleRenderer` calls `useModuleLoader("contact-form")`
3. `useModuleLoader` queries `module-registry.ts` for the registered async loader
4. Vue's `defineAsyncComponent` loads the component with `<Suspense>` boundary
5. The component receives `moduleKey` as a prop and fetches its own data

### SSR Considerations

- Module components **must be SSR-safe**: no `window`, `document`, or browser-only APIs at top level
- Use `onMounted()` for browser-only logic
- Use `useFetch` or `useAsyncData` (not raw `fetch`) for data that needs SSR hydration
- Avoid global side effects in the component's `<script setup>` block

---

## Step 4: Frontend — Admin Component

Admin components appear in the Admin SPA's module management section. They provide configuration UI for the module.

### Component

```vue
<!-- frontend/admin/components/ContactFormSettings.vue -->
<script setup lang="ts">
import { useApi } from '@/composables/useApi'

const api = useApi()
const config = ref({
  recipientEmail: '',
  subject: 'New Contact Form Submission',
  successMessage: 'Thank you for your message!',
  fields: [
    { name: 'name', label: 'Name', type: 'text', required: true },
    { name: 'email', label: 'Email', type: 'email', required: true },
    { name: 'message', label: 'Message', type: 'textarea', required: true },
  ],
})

async function save() {
  await api.put('/api/modules/contact-form/config', config.value)
}
</script>

<template>
  <div>
    <h2>Contact Form Settings</h2>
    <!-- Config form using PrimeVue components -->
    <InputText v-model="config.recipientEmail" placeholder="Recipient email" />
    <InputText v-model="config.subject" placeholder="Email subject" />
    <InputText v-model="config.successMessage" placeholder="Success message" />
    <!-- Field builder would go here -->
    <Button label="Save" @click="save" />
  </div>
</template>
```

### Entry Point

```typescript
// frontend/admin/index.ts
export { default as SettingsComponent } from './components/ContactFormSettings.vue'

export const moduleKey = 'contact-form'
export const adminRoutes = [
  {
    path: '/modules/contact-form',
    name: 'module-contact-form',
    component: () => import('./components/ContactFormSettings.vue'),
  },
]
```

---

## Step 5: Installation Flow

### What Happens When a Module Is Installed

1. **Admin uploads/selects module** → Admin UI sends install request to backend
2. **Backend validates `manifest.json`** → Checks moduleKey uniqueness, schema validity
3. **Backend registers module** → Creates `Module` entity record with status `Installed`
4. **ContentTypes are created** → Each content type in the manifest is inserted into the `ContentTypes` table (JSON schema, no migration needed)
5. **Frontend assets are registered** → Public components are registered in `module-registry.ts`, admin components are added to the admin router
6. **Module APIs become available** → Controllers under `/api/modules/{moduleKey}/` are active

### Module Lifecycle

```
Install → Installed (active)
              │
              ├─ Disable → Disabled (APIs return 404, slots show fallback)
              │                │
              │                └─ Enable → Installed
              │
              └─ Uninstall → Removed (data optionally preserved)
```

The `Module` entity tracks state:

```
Module {
  Id:        Guid
  ModuleKey: "contact-form"
  Name:      "Contact Form"
  Version:   "1.0.0"
  Status:    Installed | Disabled
}
```

---

## Step 6: Using Modules in Page Builder

Once installed, modules appear in the Admin page builder's slot assignment dropdown.

### Assigning a Module to a Slot

In the Admin UI, when editing a page:

1. Select a template slot (e.g., "sidebar")
2. Choose **Target Type: Module**
3. Select the module from the dropdown (e.g., "Contact Form")
4. Save the page

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

On the public site, `SlotRenderer` detects `targetType === 'module'` and delegates to `ModuleRenderer`, which loads the registered async component.

---

## Complete Example: Contact Form Module

### Directory Layout

```
seems-module-contact-form/
├── manifest.json
│
├── backend/
│   ├── ContactFormModule.cs              # DI registration
│   ├── Controllers/
│   │   └── ContactFormController.cs      # POST submit, GET submissions, GET/PUT config
│   ├── Models/
│   │   ├── ContactFormSubmission.cs      # Form submission DTO
│   │   └── ContactFormConfig.cs          # Configuration DTO
│   └── Services/
│       ├── IContactFormService.cs
│       └── ContactFormService.cs         # Email sending, submission storage
│
├── frontend/
│   ├── public/
│   │   ├── components/
│   │   │   └── ContactForm.vue           # SSR-safe form component
│   │   └── index.ts                      # registerModuleComponent(...)
│   └── admin/
│       ├── components/
│       │   ├── ContactFormSettings.vue   # Module config page
│       │   └── SubmissionList.vue        # View form submissions
│       └── index.ts                      # Admin routes + exports
│
└── README.md
```

### Manifest

```json
{
  "moduleKey": "contact-form",
  "name": "Contact Form",
  "version": "1.0.0",
  "description": "Configurable contact form with email notifications",
  "author": "SEEMS Modules",
  "slots": [
    { "key": "contact-form", "label": "Contact Form", "maxItems": 1 }
  ],
  "contentTypes": [
    {
      "key": "contact-form-config",
      "name": "Contact Form Configuration",
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
    { "method": "GET", "path": "/api/modules/contact-form/submissions" },
    { "method": "GET", "path": "/api/modules/contact-form/config" },
    { "method": "PUT", "path": "/api/modules/contact-form/config" }
  ]
}
```

---

## Conventions & Constraints

| Rule | Rationale |
|------|-----------|
| `moduleKey` must be kebab-case and globally unique | Used in URLs, DB keys, and component registry |
| All backend routes must start with `/api/modules/{moduleKey}/` | Namespace isolation; prevents collision with core APIs |
| ContentType keys should be prefixed with module key | Avoids collisions (e.g., `contact-form-config`, not `config`) |
| Public components must be SSR-safe | Nuxt 3 renders on the server first |
| Modules must not import from `Seems.Domain` or `Seems.Infrastructure` | Use only `Seems.Shared` contracts for loose coupling |
| Schema changes use JSON — never EF Core migrations | Module content types are schema-driven, stored as JSON in `ContentTypes.Schema` |
| All module data goes through `ContentItem` or module-owned API storage | No direct core table modifications |

---

## TypeScript Interfaces

Modules on the frontend side should conform to these types (defined in `src/frontend/public/types/module.ts`):

```typescript
interface ModuleManifest {
  moduleKey: string
  name: string
  version: string
  contentTypes?: { key: string; schema: Record<string, unknown> }[]
  apis?: { method: string; path: string }[]
}

interface ModuleComponent {
  moduleKey: string
  componentName: string
  loader: () => Promise<unknown>
}
```

---

## Backend Contract Interface

Modules that provide backend logic implement `IModuleManifest` from `Seems.Shared`:

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

Example implementation:

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

## Troubleshooting

| Problem | Cause | Fix |
|---------|-------|-----|
| Module component not rendering | Not registered in `module-registry.ts` | Call `registerModuleComponent(moduleKey, loader)` in the public entry point |
| Slot shows "Loading module..." forever | Async loader fails or times out | Check browser console; ensure the component path in `import()` is correct |
| Slot shows "Module could not be loaded" | `getModuleComponent()` returns null | Verify `moduleKey` matches between manifest, registration, and `SlotMapping.targetId` |
| Module API returns 404 | Controller route mismatch or module disabled | Check route attribute matches `/api/modules/{moduleKey}/...`; check module status is `Installed` |
| Content type not appearing in Admin | Module install didn't register content types | Verify `contentTypes` array in manifest; check `ContentTypes` table for the key |
