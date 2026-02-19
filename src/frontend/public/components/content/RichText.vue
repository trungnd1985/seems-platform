<script setup lang="ts">
defineProps<{
  data: {
    html: string
  }
}>()
</script>

<template>
  <div class="rich-text" v-html="data.html" />
</template>

<style scoped>
.rich-text :deep(h1, h2, h3, h4, h5, h6) {
  margin-top: 1.5em;
  margin-bottom: 0.5em;
}

.rich-text :deep(p) {
  margin-bottom: 1em;
}

.rich-text :deep(ul, ol) {
  padding-left: 1.5em;
  margin-bottom: 1em;
}
</style>
