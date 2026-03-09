<script setup lang="ts">
import { onMounted } from 'vue'
import { RouterView } from 'vue-router'
import Toast from 'primevue/toast'
import ConfirmDialog from 'primevue/confirmdialog'
import { useColorSchemeStore } from '@/stores/colorScheme'

const colorScheme = useColorSchemeStore()
onMounted(() => colorScheme.applyToDocument())
</script>

<template>
  <Toast position="top-right" />
  <ConfirmDialog />
  <RouterView />
</template>
