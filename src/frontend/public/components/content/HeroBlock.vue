<script setup lang="ts">
defineProps<{
  data: {
    title: string
    subtitle?: string
    backgroundImage?: string
    ctaText?: string
    ctaLink?: string
  }
}>()
</script>

<template>
  <section
    class="hero-block"
    :style="data.backgroundImage ? { backgroundImage: `url(${data.backgroundImage})` } : undefined"
  >
    <div class="hero-content">
      <h1>{{ data.title }}</h1>
      <p v-if="data.subtitle" class="hero-subtitle">{{ data.subtitle }}</p>
      <NuxtLink v-if="data.ctaText && data.ctaLink" :to="data.ctaLink" class="hero-cta">
        {{ data.ctaText }}
      </NuxtLink>
    </div>
  </section>
</template>

<style scoped>
.hero-block {
  padding: 4rem 2rem;
  background-color: #1a1a2e;
  background-size: cover;
  background-position: center;
  color: #fff;
  text-align: center;
}

.hero-content {
  max-width: 800px;
  margin: 0 auto;
}

.hero-content h1 {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.hero-subtitle {
  font-size: 1.25rem;
  opacity: 0.9;
  margin-bottom: 2rem;
}

.hero-cta {
  display: inline-block;
  padding: 0.75rem 2rem;
  background: #fff;
  color: #1a1a2e;
  border-radius: 6px;
  font-weight: 600;
}
</style>
