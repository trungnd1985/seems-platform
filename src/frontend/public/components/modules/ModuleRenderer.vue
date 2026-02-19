<script setup lang="ts">
import { useModuleLoader } from '~/composables/useModuleLoader'

const props = defineProps<{
  moduleKey: string
}>()

const component = useModuleLoader(props.moduleKey)
</script>

<template>
  <Suspense>
    <component :is="component" :module-key="moduleKey" />
    <template #fallback>
      <ModuleFallback loading />
    </template>
  </Suspense>
</template>
