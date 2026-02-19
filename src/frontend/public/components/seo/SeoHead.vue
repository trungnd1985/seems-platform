<script setup lang="ts">
import type { SeoMeta } from '~/types/page'
import { useSeo } from '~/composables/useSeo'

const props = defineProps<{
  seo: SeoMeta
}>()

useSeo(props.seo)
</script>

<template>
  <slot />
</template>
