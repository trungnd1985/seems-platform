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
  <div class="landing-page">
    <section v-if="getSlotMappings('hero').length" class="slot-hero">
      <SlotRenderer
        v-for="mapping in getSlotMappings('hero')"
        :key="mapping.targetId"
        :mapping="mapping"
      />
    </section>

    <section class="slot-main">
      <SlotRenderer
        v-for="mapping in getSlotMappings('main')"
        :key="mapping.targetId"
        :mapping="mapping"
      />
    </section>

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
.landing-page {
  width: 100%;
}

.slot-main {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem 1rem;
}
</style>
