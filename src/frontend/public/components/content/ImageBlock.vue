<script setup lang="ts">
defineProps<{
  data: {
    src: string
    alt: string
    caption?: string
  }
}>()
</script>

<template>
  <figure class="image-block">
    <NuxtImg
      :src="data.src"
      :alt="data.alt"
      loading="lazy"
      sizes="sm:100vw md:80vw lg:1200px"
    />
    <figcaption v-if="data.caption">{{ data.caption }}</figcaption>
  </figure>
</template>

<style scoped>
.image-block {
  margin: 1.5rem 0;
}

.image-block img {
  border-radius: 6px;
}

.image-block figcaption {
  margin-top: 0.5rem;
  font-size: 0.875rem;
  color: #6b7280;
  text-align: center;
}
</style>
