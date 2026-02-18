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
  <div class="standard-page">
    <section v-if="getSlotMappings('hero').length" class="slot-hero">
      <SlotRenderer
        v-for="mapping in getSlotMappings('hero')"
        :key="mapping.targetId"
        :mapping="mapping"
      />
    </section>

    <div class="standard-page-body">
      <div class="slot-main">
        <SlotRenderer
          v-for="mapping in getSlotMappings('main')"
          :key="mapping.targetId"
          :mapping="mapping"
        />
      </div>

      <aside v-if="getSlotMappings('sidebar').length" class="slot-sidebar">
        <SlotRenderer
          v-for="mapping in getSlotMappings('sidebar')"
          :key="mapping.targetId"
          :mapping="mapping"
        />
      </aside>
    </div>

    <section v-if="getSlotMappings('footer').length" class="slot-footer">
      <SlotRenderer
        v-for="mapping in getSlotMappings('footer')"
        :key="mapping.targetId"
        :mapping="mapping"
      />
    </section>
  </div>
</template>

<style scoped>
.standard-page-body {
  display: grid;
  grid-template-columns: 1fr 300px;
  gap: 2rem;
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem 1rem;
}

.slot-hero,
.slot-footer {
  width: 100%;
}

@media (max-width: 768px) {
  .standard-page-body {
    grid-template-columns: 1fr;
  }
}
</style>
