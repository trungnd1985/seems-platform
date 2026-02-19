<script setup lang="ts">
defineProps<{
  loading?: boolean
}>()
</script>

<template>
  <div class="module-fallback">
    <p v-if="loading">Loading module...</p>
    <p v-else>Module could not be loaded.</p>
  </div>
</template>

<style scoped>
.module-fallback {
  padding: 1rem;
  text-align: center;
  color: #6b7280;
  font-size: 0.875rem;
}
</style>
