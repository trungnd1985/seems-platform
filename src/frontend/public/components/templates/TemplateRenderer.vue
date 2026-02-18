<script setup lang="ts">
import type { Page, SlotMapping } from '~/types/page'
import { useTemplateResolver } from '~/composables/useTemplateResolver'

const props = defineProps<{
  page: Page
}>()

const templateComponent = computed(() =>
  useTemplateResolver(props.page.templateKey),
)
</script>

<template>
  <component
    :is="templateComponent"
    :page="page"
    :slots="page.slots"
  />
</template>
