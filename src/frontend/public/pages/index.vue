<script setup lang="ts">
import { usePageResolver } from '~/composables/usePageResolver'
import { useSeo } from '~/composables/useSeo'
import { useTemplateResolver } from '~/composables/useTemplateResolver'

const { data: page, error } = await usePageResolver('/')

if (page.value) {
  useSeo(page.value.seo)
}

const templateComponent = computed(() => {
  if (!page.value) return null
  return useTemplateResolver(page.value.templateKey)
})
</script>

<template>
  <div v-if="error" class="error-page">
    <h1>Page not found</h1>
  </div>
  <component
    v-else-if="page && templateComponent"
    :is="templateComponent"
    :page="page"
    :slots="page.slots"
  />
</template>
