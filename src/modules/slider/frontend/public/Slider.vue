<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'

interface Slide {
  id: string
  title: string
  subtitle?: string
  imageUrl: string
  ctaText?: string
  ctaLink?: string
  order: number
  status: string
}

const props = defineProps<{ moduleKey: string }>()

// Plain fetch — useFetch is a Nuxt auto-import and is NOT available in a
// standalone ESM bundle loaded via dynamic import() in modules.client.ts.
const slides = ref<Slide[]>([])

const current = ref(0)
let timer: ReturnType<typeof setInterval> | null = null

const total = computed(() => slides.value.length)

function next() {
  if (total.value > 0)
    current.value = (current.value + 1) % total.value
}

function prev() {
  if (total.value > 0)
    current.value = (current.value - 1 + total.value) % total.value
}

function goTo(index: number) {
  current.value = index
  resetTimer()
}

function resetTimer() {
  if (timer) clearInterval(timer)
  timer = setInterval(next, 5000)
}

onMounted(async () => {
  try {
    const res = await fetch(`/api/modules/${props.moduleKey}/slides`)
    if (res.ok) slides.value = await res.json()
  } catch {
    // Non-fatal: slider simply won't render
  }
  if (slides.value.length > 1)
    timer = setInterval(next, 5000)
})

onUnmounted(() => {
  if (timer) clearInterval(timer)
})
</script>

<template>
  <div v-if="slides.length" class="hero-slider">
    <div class="slides-wrapper">
      <transition-group name="fade" tag="div" class="slides-container">
        <div
          v-for="(slide, index) in slides"
          v-show="index === current"
          :key="slide.id"
          class="slide"
          :style="{ backgroundImage: `url(${slide.imageUrl})` }"
        >
          <div class="slide-overlay" />
          <div class="slide-content">
            <h1 class="slide-title">{{ slide.title }}</h1>
            <p v-if="slide.subtitle" class="slide-subtitle">{{ slide.subtitle }}</p>
            <a
              v-if="slide.ctaText && slide.ctaLink"
              :href="slide.ctaLink"
              class="slide-cta"
            >
              {{ slide.ctaText }}
            </a>
          </div>
        </div>
      </transition-group>
    </div>

    <template v-if="slides.length > 1">
      <button class="nav-arrow nav-prev" aria-label="Previous slide" @click="prev">&#8249;</button>
      <button class="nav-arrow nav-next" aria-label="Next slide" @click="next">&#8250;</button>

      <div class="dots">
        <button
          v-for="(_, index) in slides"
          :key="index"
          class="dot"
          :class="{ 'dot-active': index === current }"
          :aria-label="`Go to slide ${Number(index) + 1}`"
          @click="goTo(Number(index))"
        />
      </div>
    </template>
  </div>
</template>

<style scoped>
.hero-slider {
  position: relative;
  width: 100%;
  height: 500px;
  overflow: hidden;
  background: #1c1917;
}

.slides-wrapper,
.slides-container {
  width: 100%;
  height: 100%;
  position: relative;
}

.slide {
  position: absolute;
  inset: 0;
  background-size: cover;
  background-position: center;
  display: flex;
  align-items: center;
}

.slide-overlay {
  position: absolute;
  inset: 0;
  background: rgba(0, 0, 0, 0.42);
}

.slide-content {
  position: relative;
  z-index: 1;
  max-width: 800px;
  padding: 0 4rem;
}

.slide-title {
  font-size: 2.5rem;
  font-weight: 700;
  color: #fff;
  margin: 0 0 0.75rem;
  line-height: 1.2;
}

.slide-subtitle {
  font-size: 1.125rem;
  color: rgba(255, 255, 255, 0.82);
  margin: 0 0 1.5rem;
  line-height: 1.5;
}

.slide-cta {
  display: inline-block;
  padding: 0.75rem 1.75rem;
  background: #fff;
  color: #1c1917;
  font-weight: 600;
  font-size: 0.95rem;
  border-radius: 6px;
  text-decoration: none;
  transition: background 0.2s, color 0.2s;
}

.slide-cta:hover {
  background: #f5f5f4;
}

/* Arrows */
.nav-arrow {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  z-index: 10;
  width: 48px;
  height: 48px;
  border-radius: 50%;
  border: none;
  background: rgba(255, 255, 255, 0.2);
  color: #fff;
  font-size: 1.75rem;
  line-height: 1;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.2s;
  backdrop-filter: blur(4px);
}

.nav-arrow:hover {
  background: rgba(255, 255, 255, 0.36);
}

.nav-prev { left: 1.25rem; }
.nav-next { right: 1.25rem; }

/* Dots */
.dots {
  position: absolute;
  bottom: 1.25rem;
  left: 50%;
  transform: translateX(-50%);
  z-index: 10;
  display: flex;
  gap: 0.5rem;
}

.dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  border: 2px solid rgba(255, 255, 255, 0.7);
  background: transparent;
  cursor: pointer;
  padding: 0;
  transition: background 0.2s;
}

.dot-active {
  background: #fff;
  border-color: #fff;
}

/* Fade transition */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.5s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
