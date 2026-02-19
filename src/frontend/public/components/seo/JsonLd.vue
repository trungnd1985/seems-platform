<script setup lang="ts">
const props = defineProps<{
  type: 'WebPage' | 'Article' | 'Organization' | 'BreadcrumbList'
  data: Record<string, unknown>
}>()

const jsonLd = computed(() => ({
  '@context': 'https://schema.org',
  '@type': props.type,
  ...props.data,
}))

useHead({
  script: [
    {
      type: 'application/ld+json',
      innerHTML: JSON.stringify(jsonLd.value),
    },
  ],
})
</script>

<template>
  <slot />
</template>
