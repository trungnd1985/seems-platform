<script setup lang="ts">
import type { Page, SlotMapping } from '~/types/page'

const props = defineProps<{
  page: Page
  slots: SlotMapping[]
}>()

function getSlotMappings(slotKey: string): SlotMapping[] {
  return props.slots
    .filter((s) => s.slotKey === slotKey)
    .sort((a, b) => a.order - b.order)
}
</script>

<template>
  <article class="blog-post">
    <header class="blog-post-header">
      <h1>{{ page.title }}</h1>
    </header>

    <div class="blog-post-content">
      <SlotRenderer
        v-for="mapping in getSlotMappings('main')"
        :key="mapping.targetId"
        :mapping="mapping"
      />
    </div>

    <footer v-if="getSlotMappings('footer').length" class="blog-post-footer">
      <SlotRenderer
        v-for="mapping in getSlotMappings('footer')"
        :key="mapping.targetId"
        :mapping="mapping"
      />
    </footer>
  </article>
</template>

<style scoped>
.blog-post {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem 1rem;
}

.blog-post-header {
  margin-bottom: 2rem;
}

.blog-post-header h1 {
  font-size: 2.25rem;
  line-height: 1.2;
}

.blog-post-footer {
  margin-top: 3rem;
  padding-top: 2rem;
  border-top: 1px solid #e5e5e5;
}
</style>
