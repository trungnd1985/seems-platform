<script setup lang="ts">
import type { ContentItem } from '~/types/content'
import { resolveContentComponent } from '~/utils/content-type-map'

const props = defineProps<{
  item: ContentItem
}>()

const component = computed(() =>
  resolveContentComponent(props.item.contentTypeKey),
)
</script>

<template>
  <component
    v-if="component"
    :is="component"
    :data="item.data"
  />
  <div v-else class="content-unknown">
    <p>Unknown content type: {{ item.contentTypeKey }}</p>
  </div>
</template>

<style scoped>
.content-unknown {
  padding: 1rem;
  background: #fef3c7;
  border: 1px solid #f59e0b;
  border-radius: 4px;
  color: #92400e;
}
</style>
