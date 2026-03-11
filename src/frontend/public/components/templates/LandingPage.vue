<script setup lang="ts">
import type { Page, SlotMapping } from '~/types/page'

const props = defineProps<{
  page: Page
  slots: SlotMapping[]
}>()

function getSlot(slotKey: string): SlotMapping[] {
  return props.slots
    .filter((s) => s.slotKey === slotKey)
    .sort((a, b) => a.order - b.order)
}
</script>

<template>
  <div class="landing-page">
    <!-- Full-width hero -->
    <section v-if="getSlot('hero').length || getSlot('slider').length" class="slot-hero">
      <SlotRenderer
        v-for="mapping in getSlot('hero')"
        :key="mapping.targetId"
        :mapping="mapping"
      />
      <SlotRenderer
        v-for="mapping in getSlot('slider')"
        :key="mapping.targetId"
        :mapping="mapping"
      />
    </section>

    <!-- Main content (full-width, no sidebar) -->
    <div v-if="getSlot('main').length" class="slot-main">
      <div class="container">
        <SlotRenderer
          v-for="mapping in getSlot('main')"
          :key="mapping.targetId"
          :mapping="mapping"
        />
      </div>
    </div>

    <!-- Full-width footer widgets -->
    <section v-if="getSlot('footer').length" class="slot-footer-widgets">
      <SlotRenderer
        v-for="mapping in getSlot('footer')"
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

.slot-hero {
  width: 100%;
}

.slot-main {
  padding: 70px 0;
}

.slot-footer-widgets {
  width: 100%;
  background: var(--color-light, #f7f7f7);
  border-top: 1px solid var(--color-border, #e9e9e9);
}

@media (max-width: 768px) {
  .slot-main { padding: 40px 0; }
}
</style>
