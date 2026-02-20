<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import Menu from 'primevue/menu'
import type { MenuItem } from 'primevue/menuitem'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()

const menuItems = computed<MenuItem[]>(() => [
  {
    label: 'Dashboard',
    icon: 'pi pi-home',
    command: () => router.push({ name: 'dashboard' }),
  },
  { separator: true },
  {
    label: 'Content',
    items: [
      {
        label: 'Pages',
        icon: 'pi pi-file',
        command: () => router.push({ name: 'pages' }),
      },
      {
        label: 'Content Types',
        icon: 'pi pi-th-large',
        command: () => router.push({ name: 'content-types' }),
      },
      {
        label: 'Content Items',
        icon: 'pi pi-list',
        command: () => router.push({ name: 'content-items' }),
      },
      {
        label: 'Media',
        icon: 'pi pi-images',
        command: () => router.push({ name: 'media' }),
      },
    ],
  },
  { separator: true },
  {
    label: 'Appearance',
    items: [
      {
        label: 'Templates',
        icon: 'pi pi-objects-column',
        command: () => router.push({ name: 'templates' }),
      },
      {
        label: 'Themes',
        icon: 'pi pi-palette',
        command: () => router.push({ name: 'themes' }),
      },
    ],
  },
  { separator: true },
  {
    label: 'System',
    items: [
      {
        label: 'Modules',
        icon: 'pi pi-box',
        command: () => router.push({ name: 'modules' }),
      },
      ...(auth.user?.roles.includes('Admin')
        ? [
            {
              label: 'Users',
              icon: 'pi pi-users',
              command: () => router.push({ name: 'users' }),
            },
            {
              label: 'Roles',
              icon: 'pi pi-shield',
              command: () => router.push({ name: 'roles' }),
            },
          ]
        : []),
      {
        label: 'Settings',
        icon: 'pi pi-cog',
        command: () => router.push({ name: 'settings' }),
      },
    ],
  },
])
</script>

<template>
  <aside class="admin-sidebar">
    <div class="sidebar-brand">
      <span class="brand-text">SEEMS</span>
    </div>
    <Menu :model="menuItems" class="sidebar-menu" />
  </aside>
</template>

<style scoped>
.admin-sidebar {
  width: 260px;
  height: 100%;
  background: var(--p-surface-0);
  border-right: 1px solid var(--p-surface-200);
  display: flex;
  flex-direction: column;
  overflow-y: auto;
}

.sidebar-brand {
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid var(--p-surface-200);
}

.brand-text {
  font-size: 1.25rem;
  font-weight: 700;
  letter-spacing: 0.05em;
  color: var(--p-primary-color);
}

.sidebar-menu {
  border: none;
  width: 100%;
}
</style>
