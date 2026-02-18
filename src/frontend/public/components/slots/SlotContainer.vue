<script setup lang="ts">
defineProps<{
  class?: string
}>()
</script>

<template>
  <div class="slot-container" :class="$props.class">
    <slot />
  </div>
</template>

<style scoped>
.slot-container {
  width: 100%;
}
</style>
