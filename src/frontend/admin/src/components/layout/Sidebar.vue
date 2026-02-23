<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useSidebar } from '@/composables/useSidebar'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const { collapsed, mobileOpen } = useSidebar()

interface NavItem {
  label: string
  icon: string
  routeName: string
  adminOnly?: boolean
}

interface NavGroup {
  label: string
  items: NavItem[]
}

const navGroups: NavGroup[] = [
  {
    label: 'Content',
    items: [
      { label: 'Pages', icon: 'pi pi-file', routeName: 'pages' },
      { label: 'Content Types', icon: 'pi pi-th-large', routeName: 'content-types' },
      { label: 'Content Items', icon: 'pi pi-list', routeName: 'content-items' },
      { label: 'Categories', icon: 'pi pi-tag', routeName: 'content-categories' },
      { label: 'Media', icon: 'pi pi-images', routeName: 'media' },
    ],
  },
  {
    label: 'Appearance',
    items: [
      { label: 'Templates', icon: 'pi pi-objects-column', routeName: 'templates' },
      { label: 'Themes', icon: 'pi pi-palette', routeName: 'themes' },
    ],
  },
  {
    label: 'System',
    items: [
      { label: 'Modules', icon: 'pi pi-box', routeName: 'modules' },
      { label: 'Users', icon: 'pi pi-users', routeName: 'users', adminOnly: true },
      { label: 'Roles', icon: 'pi pi-shield', routeName: 'roles', adminOnly: true },
      { label: 'Settings', icon: 'pi pi-cog', routeName: 'settings' },
    ],
  },
]

const isAdmin = computed(() => auth.user?.roles.includes('Admin') ?? false)

const visibleGroups = computed(() =>
  navGroups
    .map((group) => ({
      ...group,
      items: group.items.filter((item) => !item.adminOnly || isAdmin.value),
    }))
    .filter((group) => group.items.length > 0),
)

function isActive(routeName: string): boolean {
  return route.name === routeName
}

function navigate(routeName: string): void {
  void router.push({ name: routeName })
  mobileOpen.value = false
}

function toggleCollapse(): void {
  collapsed.value = !collapsed.value
}
</script>

<template>
  <aside
    class="admin-sidebar"
    :class="{
      'sidebar-collapsed': collapsed,
      'sidebar-mobile-open': mobileOpen,
    }"
  >
    <!-- Mobile backdrop: position:fixed so it doesn't affect sidebar layout -->
    <Transition name="fade">
      <div v-if="mobileOpen" class="sidebar-backdrop" @click="mobileOpen = false" />
    </Transition>

    <!-- Brand -->
    <div class="sidebar-brand">
      <span class="brand-icon">S</span>
      <Transition name="fade-text">
        <span v-if="!collapsed" class="brand-text">SEEMS</span>
      </Transition>
    </div>

    <!-- Nav -->
    <nav class="sidebar-nav">
      <!-- Dashboard -->
      <button
        class="nav-item"
        :class="{ 'nav-item-active': isActive('dashboard') }"
        v-tooltip="{ value: 'Dashboard', position: 'right', disabled: !collapsed }"
        @click="navigate('dashboard')"
      >
        <i class="pi pi-home nav-icon" />
        <span v-if="!collapsed" class="nav-label">Dashboard</span>
      </button>

      <!-- Groups -->
      <template v-for="group in visibleGroups">
        <div
          v-if="!collapsed"
          :key="`label-${group.label}`"
          class="nav-group-label"
        >{{ group.label }}</div>
        <div v-else :key="`divider-${group.label}`" class="nav-group-divider" />

        <button
          v-for="item in group.items"
          :key="item.routeName"
          class="nav-item"
          :class="{ 'nav-item-active': isActive(item.routeName) }"
          v-tooltip="{ value: item.label, position: 'right', disabled: !collapsed }"
          @click="navigate(item.routeName)"
        >
          <i :class="[item.icon, 'nav-icon']" />
          <span v-if="!collapsed" class="nav-label">{{ item.label }}</span>
        </button>
      </template>
    </nav>

    <!-- Collapse toggle (desktop only) -->
    <button class="sidebar-collapse-btn" @click="toggleCollapse">
      <i :class="['pi', collapsed ? 'pi-angle-right' : 'pi-angle-left']" />
    </button>
  </aside>
</template>

<style scoped>
/* Backdrop — mobile only */
.sidebar-backdrop {
  display: none;
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  z-index: 99;
}

@media (max-width: 768px) {
  .sidebar-backdrop {
    display: block;
  }
}

/* Sidebar shell */
.admin-sidebar {
  width: 260px;
  height: 100%;
  background: var(--p-surface-50);
  border-right: 1px solid var(--p-surface-200);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  flex-shrink: 0;
  transition: width 0.22s ease;
}

.sidebar-collapsed {
  width: 64px;
}

@media (max-width: 768px) {
  .admin-sidebar {
    position: fixed;
    left: -280px;
    top: 0;
    width: 260px !important; /* always full-width on mobile */
    height: 100dvh;
    z-index: 100;
    box-shadow: none;
    transition: left 0.25s ease, box-shadow 0.25s ease;
  }

  .admin-sidebar.sidebar-mobile-open {
    left: 0;
    box-shadow: 4px 0 20px rgba(0, 0, 0, 0.15);
  }

  .sidebar-collapse-btn {
    display: none !important;
  }
}

/* Brand */
.sidebar-brand {
  height: 56px;
  padding: 0 1rem;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  border-bottom: 1px solid var(--p-surface-200);
  flex-shrink: 0;
  overflow: hidden;
}

.brand-icon {
  width: 32px;
  height: 32px;
  background: var(--p-primary-color);
  color: #fff;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 1rem;
  flex-shrink: 0;
}

.brand-text {
  font-size: 1.125rem;
  font-weight: 700;
  letter-spacing: 0.05em;
  color: var(--p-surface-900);
  white-space: nowrap;
}

/* Nav */
.sidebar-nav {
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 0.5rem;
  display: flex;
  flex-direction: column;
  gap: 1px;
}

.nav-group-label {
  font-size: 0.7rem;
  font-weight: 600;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--p-surface-400);
  padding: 0.875rem 0.625rem 0.3rem;
  white-space: nowrap;
}

.nav-group-divider {
  height: 1px;
  background: var(--p-surface-200);
  margin: 0.5rem 0.25rem;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.575rem 0.625rem;
  border-radius: 8px;
  border: none;
  background: transparent;
  color: var(--p-surface-600);
  cursor: pointer;
  width: 100%;
  text-align: left;
  font-size: 0.875rem;
  font-family: inherit;
  transition:
    background 0.15s,
    color 0.15s;
  white-space: nowrap;
}

/* Collapsed: center the icon */
.sidebar-collapsed .nav-item {
  justify-content: center;
  gap: 0;
  padding: 0.575rem;
}

.nav-item:hover {
  background: var(--p-surface-100);
  color: var(--p-surface-900);
}

.nav-item-active {
  background: color-mix(in srgb, var(--p-primary-color) 10%, transparent);
  color: var(--p-primary-color);
  font-weight: 600;
}

.nav-item-active:hover {
  background: color-mix(in srgb, var(--p-primary-color) 16%, transparent);
}

.nav-icon {
  font-size: 1rem;
  flex-shrink: 0;
  width: 1rem;
  text-align: center;
}

.nav-label {
  overflow: hidden;
  text-overflow: ellipsis;
}

/* Collapse button */
.sidebar-collapse-btn {
  height: 44px;
  border: none;
  border-top: 1px solid var(--p-surface-200);
  background: transparent;
  color: var(--p-surface-400);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition:
    background 0.15s,
    color 0.15s;
}

.sidebar-collapse-btn:hover {
  background: var(--p-surface-50);
  color: var(--p-surface-700);
}

/* Transitions */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.18s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.fade-text-enter-active,
.fade-text-leave-active {
  transition: opacity 0.15s ease;
}
.fade-text-enter-from,
.fade-text-leave-to {
  opacity: 0;
}
</style>
