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
  <div class="standard-page">
    <!-- Full-width slider/hero -->
    <section v-if="getSlot('slider').length" class="slot-slider">
      <SlotRenderer
        v-for="mapping in getSlot('slider')"
        :key="mapping.targetId"
        :mapping="mapping"
      />
    </section>

    <!-- Intro bar (optional, full-width accent strip) -->
    <section v-if="getSlot('intro').length" class="slot-intro">
      <div class="container">
        <SlotRenderer
          v-for="mapping in getSlot('intro')"
          :key="mapping.targetId"
          :mapping="mapping"
        />
      </div>
    </section>

    <!-- Main content + optional sidebar -->
    <div class="page-body">
      <div class="container">
        <div class="page-body-inner" :class="{ 'has-sidebar': getSlot('sidebar').length }">
          <main class="slot-main">
            <SlotRenderer
              v-for="mapping in getSlot('main')"
              :key="mapping.targetId"
              :mapping="mapping"
            />
            <SlotRenderer
              v-for="mapping in getSlot('body')"
              :key="mapping.targetId"
              :mapping="mapping"
            />
          </main>

          <aside v-if="getSlot('sidebar').length" class="slot-sidebar">
            <SlotRenderer
              v-for="mapping in getSlot('sidebar')"
              :key="mapping.targetId"
              :mapping="mapping"
            />
          </aside>
        </div>
      </div>
    </div>

    <!-- Full-width footer widget area -->
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
.standard-page {
  width: 100%;
}

/* ─── Slider ─── */
.slot-slider {
  width: 100%;
}

/* ─── Intro Bar ─── */
.slot-intro {
  background: #0088cc;
  color: #fff;
  padding: 18px 0;
}

/* ─── Page Body ─── */
.page-body {
  padding: 60px 0;
}

.page-body-inner {
  display: grid;
  grid-template-columns: 1fr;
  gap: 2.5rem;
}

.page-body-inner.has-sidebar {
  grid-template-columns: 1fr 300px;
}

/* ─── Sidebar ─── */
.slot-sidebar {
  min-width: 0;
}

/* ─── Footer Widgets ─── */
.slot-footer-widgets {
  width: 100%;
  background: var(--color-light, #f7f7f7);
  border-top: 1px solid var(--color-border, #e9e9e9);
}

/* ─── Responsive ─── */
@media (max-width: 991px) {
  .page-body-inner.has-sidebar {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .page-body { padding: 40px 0; }
}
</style>
