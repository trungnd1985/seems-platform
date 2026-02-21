<script setup lang="ts">
import { useDefaultPageResolver } from '~/composables/useDefaultPageResolver'
import { useSeo } from '~/composables/useSeo'
import { useTemplateResolver } from '~/composables/useTemplateResolver'
import { useTheme } from '~/composables/useTheme'

const { data: page, error } = await useDefaultPageResolver()

if (page.value) {
  useSeo(page.value.seo)
}

useTheme(page.value?.themeKey)

const templateComponent = computed(() => {
  if (!page.value) return null
  return useTemplateResolver(page.value.templateKey, page.value.themeKey)
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
