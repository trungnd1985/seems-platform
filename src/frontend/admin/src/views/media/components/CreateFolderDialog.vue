<script setup lang="ts">
import { ref, watch } from 'vue'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Button from 'primevue/button'
import { useToast } from 'primevue/usetoast'
import { useMedia } from '@/composables/useMedia'
import type { MediaFolder } from '@/types/media'

const props = defineProps<{
  visible: boolean
  parentId: string | null
}>()

const emit = defineEmits<{
  'update:visible': [val: boolean]
  created: [folder: MediaFolder]
}>()

const { createFolder } = useMedia()
const toast = useToast()

const name = ref('')
const loading = ref(false)
const error = ref('')

watch(
  () => props.visible,
  (v) => {
    if (v) {
      name.value = ''
      error.value = ''
    }
  },
)

async function submit() {
  if (!name.value.trim()) {
    error.value = 'Folder name is required.'
    return
  }
  loading.value = true
  try {
    const folder = await createFolder(name.value.trim(), props.parentId)
    emit('created', folder)
    emit('update:visible', false)
  } catch (e: unknown) {
    const msg = (e as { response?: { data?: { message?: string } } })?.response?.data?.message
    toast.add({ severity: 'error', summary: 'Error', detail: msg || 'Failed to create folder', life: 4000 })
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <Dialog
    :visible="visible"
    @update:visible="emit('update:visible', $event)"
    modal
    header="New Folder"
    :style="{ width: '360px' }"
  >
    <div class="flex flex-col gap-2">
      <label class="font-medium text-sm">Folder name</label>
      <InputText
        v-model="name"
        placeholder="e.g. Blog images"
        autofocus
        :class="{ 'p-invalid': error }"
        @keyup.enter="submit"
      />
      <small v-if="error" class="text-red-500">{{ error }}</small>
    </div>

    <template #footer>
      <Button label="Cancel" severity="secondary" text @click="emit('update:visible', false)" />
      <Button label="Create" :loading="loading" @click="submit" />
    </template>
  </Dialog>
</template>
