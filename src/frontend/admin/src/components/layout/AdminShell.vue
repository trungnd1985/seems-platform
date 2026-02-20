<script setup lang="ts">
import { RouterView } from 'vue-router'
import Sidebar from './Sidebar.vue'
import Topbar from './Topbar.vue'
</script>

<template>
  <div class="admin-shell">
    <Sidebar />
    <div class="admin-main">
      <Topbar />
      <main class="admin-content">
        <RouterView />
      </main>
    </div>
  </div>
</template>

<style scoped>
.admin-shell {
  display: flex;
  height: 100%;
  overflow: hidden;
}

.admin-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  min-width: 0;
}

.admin-content {
  flex: 1;
  padding: 1.5rem;
  overflow-y: auto;
  background: var(--p-surface-100);
}
</style>
