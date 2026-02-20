<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useSidebar } from '@/composables/useSidebar'
import Button from 'primevue/button'
import Menu from 'primevue/menu'
import Breadcrumb from 'primevue/breadcrumb'
import type { MenuItem } from 'primevue/menuitem'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const { mobileOpen } = useSidebar()

const userMenu = ref()

// Route name → breadcrumb metadata
const routeMeta: Record<string, { label: string; group?: string }> = {
  dashboard: { label: 'Dashboard' },
  pages: { label: 'Pages', group: 'Content' },
  'content-types': { label: 'Content Types', group: 'Content' },
  'content-items': { label: 'Content Items', group: 'Content' },
  media: { label: 'Media', group: 'Content' },
  templates: { label: 'Templates', group: 'Appearance' },
  themes: { label: 'Themes', group: 'Appearance' },
  modules: { label: 'Modules', group: 'System' },
  users: { label: 'Users', group: 'System' },
  roles: { label: 'Roles', group: 'System' },
  settings: { label: 'Settings', group: 'System' },
}

const breadcrumbHome: MenuItem = {
  icon: 'pi pi-home',
  route: { name: 'dashboard' },
}

const breadcrumbItems = computed<MenuItem[]>(() => {
  const name = route.name as string
  const meta = routeMeta[name]
  if (!meta || name === 'dashboard') return []
  const items: MenuItem[] = []
  if (meta.group) items.push({ label: meta.group })
  items.push({ label: meta.label })
  return items
})

const userMenuItems = ref<MenuItem[]>([
  {
    label: 'Logout',
    icon: 'pi pi-sign-out',
    command: () => {
      auth.logout()
      void router.push({ name: 'login' })
    },
  },
])

function toggleUserMenu(event: Event): void {
  userMenu.value.toggle(event)
}
</script>

<template>
  <header class="admin-topbar">
    <div class="topbar-left">
      <!-- Mobile hamburger -->
      <Button
        class="mobile-menu-btn"
        icon="pi pi-bars"
        text
        severity="secondary"
        aria-label="Open menu"
        @click="mobileOpen = true"
      />
      <!-- Breadcrumb -->
      <Breadcrumb :home="breadcrumbHome" :model="breadcrumbItems" class="topbar-breadcrumb" />
    </div>

    <div class="topbar-right">
      <Button
        type="button"
        :label="auth.user?.displayName ?? 'Admin'"
        icon="pi pi-user"
        text
        severity="secondary"
        @click="toggleUserMenu"
      />
      <Menu ref="userMenu" :model="userMenuItems" popup />
    </div>
  </header>
</template>

<style scoped>
.admin-topbar {
  height: 56px;
  padding: 0 1.25rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid var(--p-surface-200);
  background: var(--p-surface-50);
  flex-shrink: 0;
  gap: 1rem;
}

.topbar-left {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  min-width: 0;
}

/* Hamburger: hidden on desktop, visible on mobile */
.mobile-menu-btn {
  display: none;
  flex-shrink: 0;
}

@media (max-width: 768px) {
  .mobile-menu-btn {
    display: inline-flex;
  }
}

/* Strip PrimeVue Breadcrumb's default chrome */
.topbar-breadcrumb {
  padding: 0;
  background: transparent;
  border: none;
}

:deep(.p-breadcrumb) {
  background: transparent;
  border: none;
  padding: 0;
}

:deep(.p-breadcrumb-list) {
  gap: 0.25rem;
}

:deep(.p-breadcrumb-item-label) {
  font-size: 0.875rem;
  color: var(--p-surface-500);
}

:deep(.p-breadcrumb-item:last-child .p-breadcrumb-item-label) {
  color: var(--p-surface-800);
  font-weight: 500;
}

.topbar-right {
  display: flex;
  align-items: center;
  flex-shrink: 0;
}
</style>
