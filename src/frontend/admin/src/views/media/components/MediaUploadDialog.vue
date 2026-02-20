<script setup lang="ts">
import { ref } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import ProgressBar from 'primevue/progressbar'
import { useToast } from 'primevue/usetoast'
import { useMedia } from '@/composables/useMedia'
import type { MediaItem } from '@/types/media'

const props = defineProps<{
  visible: boolean
  folderId: string | null
}>()

const emit = defineEmits<{
  'update:visible': [val: boolean]
  uploaded: [items: MediaItem[]]
}>()

const { uploadMedia } = useMedia()
const toast = useToast()

type UploadEntry = {
  file: File
  progress: number
  status: 'pending' | 'uploading' | 'done' | 'error'
  error?: string
}

const entries = ref<UploadEntry[]>([])
const isDragging = ref(false)
const fileInput = ref<HTMLInputElement>()

function onDrop(e: DragEvent) {
  isDragging.value = false
  const files = Array.from(e.dataTransfer?.files ?? [])
  addFiles(files)
}

function onFileChange(e: Event) {
  const files = Array.from((e.target as HTMLInputElement).files ?? [])
  addFiles(files)
  if (fileInput.value) fileInput.value.value = ''
}

function addFiles(files: File[]) {
  for (const file of files) {
    entries.value.push({ file, progress: 0, status: 'pending' })
  }
}

const uploading = ref(false)

async function startUpload() {
  if (uploading.value) return
  uploading.value = true
  const pending = entries.value.filter((e) => e.status === 'pending')
  const uploaded: MediaItem[] = []

  for (const entry of pending) {
    entry.status = 'uploading'
    try {
      const item = await uploadMedia(entry.file, props.folderId, (pct) => {
        entry.progress = pct
      })
      entry.status = 'done'
      entry.progress = 100
      uploaded.push(item)
    } catch (err: unknown) {
      entry.status = 'error'
      entry.error = (err as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Upload failed'
    }
  }

  uploading.value = false

  if (uploaded.length > 0) {
    toast.add({ severity: 'success', summary: 'Uploaded', detail: `${uploaded.length} file(s) uploaded`, life: 3000 })
    emit('uploaded', uploaded)
  }
}

function clearDone() {
  entries.value = entries.value.filter((e) => e.status !== 'done')
}

function close() {
  entries.value = []
  emit('update:visible', false)
}
</script>

<template>
  <Dialog
    :visible="visible"
    @update:visible="close"
    modal
    header="Upload Files"
    :style="{ width: '520px' }"
  >
    <!-- Drop zone -->
    <div
      class="upload-dropzone"
      :class="{ 'upload-dropzone--active': isDragging }"
      @dragover.prevent="isDragging = true"
      @dragleave.prevent="isDragging = false"
      @drop.prevent="onDrop"
      @click="fileInput?.click()"
    >
      <i class="pi pi-cloud-upload text-3xl text-muted-color" />
      <p class="mt-2 text-sm text-muted-color">
        Drag & drop files here or <span class="text-primary font-medium">browse</span>
      </p>
    </div>
    <input ref="fileInput" type="file" multiple class="hidden" @change="onFileChange" />

    <!-- File list -->
    <ul v-if="entries.length" class="mt-3 flex flex-col gap-2">
      <li
        v-for="(entry, i) in entries"
        :key="i"
        class="flex flex-col gap-1 text-sm border border-surface rounded p-2"
      >
        <div class="flex items-center justify-between gap-2">
          <span class="truncate font-medium">{{ entry.file.name }}</span>
          <i
            v-if="entry.status === 'done'"
            class="pi pi-check-circle text-green-500 shrink-0"
          />
          <i
            v-else-if="entry.status === 'error'"
            class="pi pi-times-circle text-red-500 shrink-0"
          />
        </div>
        <ProgressBar
          v-if="entry.status === 'uploading'"
          :value="entry.progress"
          class="h-1"
        />
        <small v-if="entry.status === 'error'" class="text-red-500">{{ entry.error }}</small>
      </li>
    </ul>

    <template #footer>
      <Button label="Clear done" severity="secondary" text :disabled="!entries.some(e => e.status === 'done')" @click="clearDone" />
      <Button label="Cancel" severity="secondary" text @click="close" />
      <Button
        label="Upload"
        icon="pi pi-upload"
        :loading="uploading"
        :disabled="!entries.some(e => e.status === 'pending')"
        @click="startUpload"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.upload-dropzone {
  border: 2px dashed var(--p-content-border-color);
  border-radius: var(--p-border-radius-md);
  padding: 2rem;
  text-align: center;
  cursor: pointer;
  transition: border-color 0.2s, background 0.2s;
}
.upload-dropzone:hover,
.upload-dropzone--active {
  border-color: var(--p-primary-color);
  background: var(--p-primary-50, #f0f9ff);
}
</style>
