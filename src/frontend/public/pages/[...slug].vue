<script setup lang="ts">
import { usePageResolver } from '~/composables/usePageResolver'
import { useSeo } from '~/composables/useSeo'
import { useTemplateResolver } from '~/composables/useTemplateResolver'

const route = useRoute()
const slug = computed(() => {
  const parts = route.params.slug
  return Array.isArray(parts) ? parts.join('/') : parts
})

const { data: page, error } = await usePageResolver(slug.value)

if (error.value) {
  throw createError({ statusCode: 404, statusMessage: 'Page not found' })
}

if (page.value) {
  useSeo(page.value.seo)
}

const templateComponent = computed(() => {
  if (!page.value) return null
  return useTemplateResolver(page.value.templateKey)
})
</script>

<template>
  <component
    v-if="page && templateComponent"
    :is="templateComponent"
    :page="page"
    :slots="page.slots"
  />
</template>
