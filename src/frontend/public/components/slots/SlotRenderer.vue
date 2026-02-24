<script setup lang="ts">
import type { SlotMapping } from '~/types/page'
import { useContentItem } from '~/composables/useContentItem'
import { useModuleLoader } from '~/composables/useModuleLoader'

const props = defineProps<{
  mapping: SlotMapping
}>()

const isContent = computed(() => props.mapping.targetType === 'Content')
const isModule = computed(() => props.mapping.targetType === 'Module')

const { data: contentItem } = isContent.value
  ? await useContentItem(props.mapping.targetId)
  : { data: ref(null) }

const moduleComponent = isModule.value
  ? useModuleLoader(props.mapping.targetId)
  : null
</script>

<template>
  <SlotContainer>
    <ContentRenderer
      v-if="isContent && contentItem"
      :item="contentItem"
    />
    <component
      v-else-if="isModule && moduleComponent"
      :is="moduleComponent"
      :module-key="mapping.targetId"
    />
  </SlotContainer>
</template>
