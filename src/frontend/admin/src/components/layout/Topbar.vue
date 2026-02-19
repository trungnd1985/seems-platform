<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import Button from 'primevue/button'
import Menu from 'primevue/menu'
import type { MenuItem } from 'primevue/menuitem'

const router = useRouter()
const auth = useAuthStore()
const userMenu = ref()

const userMenuItems = ref<MenuItem[]>([
  {
    label: 'Logout',
    icon: 'pi pi-sign-out',
    command: () => {
      auth.logout()
      router.push({ name: 'login' })
    },
  },
])

function toggleUserMenu(event: Event) {
  userMenu.value.toggle(event)
}
</script>

<template>
  <header class="admin-topbar">
    <div class="topbar-left">
      <slot name="breadcrumb" />
    </div>
    <div class="topbar-right">
      <Button
        type="button"
        :label="auth.user?.displayName ?? 'Admin'"
        icon="pi pi-user"
        text
        @click="toggleUserMenu"
      />
      <Menu ref="userMenu" :model="userMenuItems" popup />
    </div>
  </header>
</template>

<style scoped>
.admin-topbar {
  height: 56px;
  padding: 0 1.5rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid var(--p-surface-200);
  background: var(--p-surface-0);
}
</style>
