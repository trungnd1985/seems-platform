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
  folder: MediaFolder | null
}>()

const emit = defineEmits<{
  'update:visible': [val: boolean]
  renamed: [folder: MediaFolder]
}>()

const { renameFolder } = useMedia()
const toast = useToast()

const name = ref('')
const loading = ref(false)

watch(
  () => props.folder,
  (f) => { if (f) name.value = f.name },
  { immediate: true },
)

async function submit() {
  if (!props.folder || !name.value.trim()) return
  loading.value = true
  try {
    const updated = await renameFolder(props.folder.id, name.value.trim())
    emit('renamed', updated)
    emit('update:visible', false)
  } catch (e: unknown) {
    const msg = (e as { response?: { data?: { message?: string } } })?.response?.data?.message
    toast.add({ severity: 'error', summary: 'Error', detail: msg || 'Failed to rename folder', life: 4000 })
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
    header="Rename Folder"
    :style="{ width: '360px' }"
  >
    <div class="flex flex-col gap-2">
      <label class="font-medium text-sm">New name</label>
      <InputText v-model="name" autofocus @keyup.enter="submit" />
    </div>

    <template #footer>
      <Button label="Cancel" severity="secondary" text @click="emit('update:visible', false)" />
      <Button label="Rename" :loading="loading" @click="submit" />
    </template>
  </Dialog>
</template>
