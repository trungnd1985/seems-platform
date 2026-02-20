<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import type { MediaItem } from '@/types/media'

const props = defineProps<{
  visible: boolean
  item: MediaItem | null
}>()

const emit = defineEmits<{
  'update:visible': [val: boolean]
  deleted: [id: string]
}>()

const deleteVisible = ref(false)

function isImage(mimeType: string) {
  return mimeType.startsWith('image/')
}

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleString()
}

function copyUrl() {
  if (props.item) navigator.clipboard.writeText(props.item.url)
}

function confirmDelete() {
  deleteVisible.value = true
}

function executeDelete() {
  if (!props.item) return
  deleteVisible.value = false
  emit('update:visible', false)
  emit('deleted', props.item.id)
}
</script>

<template>
  <Dialog
    :visible="visible"
    @update:visible="emit('update:visible', $event)"
    modal
    :header="item?.originalName ?? 'Preview'"
    :style="{ width: '640px' }"
  >
    <template v-if="item">
      <!-- Preview -->
      <div class="preview-area">
        <img
          v-if="isImage(item.mimeType)"
          :src="item.url"
          :alt="item.altText || item.originalName"
          class="preview-image"
        />
        <div v-else class="preview-icon">
          <i class="pi pi-file text-6xl text-muted-color" />
          <p class="text-muted-color mt-2">{{ item.mimeType }}</p>
        </div>
      </div>

      <!-- Meta -->
      <div class="meta-grid mt-4">
        <div class="meta-row">
          <span class="meta-label">File name</span>
          <span>{{ item.originalName }}</span>
        </div>
        <div class="meta-row">
          <span class="meta-label">Size</span>
          <span>{{ formatSize(item.size) }}</span>
        </div>
        <div class="meta-row">
          <span class="meta-label">Type</span>
          <Tag :value="item.mimeType" severity="secondary" />
        </div>
        <div class="meta-row">
          <span class="meta-label">Owner</span>
          <span>{{ item.ownerEmail ?? item.ownerId }}</span>
        </div>
        <div class="meta-row">
          <span class="meta-label">Uploaded</span>
          <span>{{ formatDate(item.createdAt) }}</span>
        </div>
        <div class="meta-row">
          <span class="meta-label">URL</span>
          <div class="flex items-center gap-2 min-w-0">
            <a :href="item.url" target="_blank" class="text-primary truncate text-sm">{{ item.url }}</a>
            <Button icon="pi pi-copy" text size="small" @click="copyUrl" />
          </div>
        </div>
      </div>
    </template>

    <template #footer>
      <Button label="Delete" severity="danger" outlined icon="pi pi-trash" @click="confirmDelete" />
      <Button label="Close" severity="secondary" text @click="emit('update:visible', false)" />
    </template>
  </Dialog>

  <!-- Inline delete confirmation — plain Dialog per PrimeVue v4 rules -->
  <Dialog
    :visible="deleteVisible"
    @update:visible="deleteVisible = $event"
    modal
    header="Delete file"
    :style="{ width: '360px' }"
  >
    <p class="text-sm">
      Are you sure you want to delete <strong>{{ item?.originalName }}</strong>? This cannot be undone.
    </p>
    <template #footer>
      <Button label="Cancel" severity="secondary" text @click="deleteVisible = false" />
      <Button label="Delete" severity="danger" @click="executeDelete" />
    </template>
  </Dialog>
</template>

<style scoped>
.preview-area {
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--p-content-hover-background);
  border-radius: var(--p-border-radius-md);
  min-height: 200px;
  overflow: hidden;
}
.preview-image {
  max-width: 100%;
  max-height: 340px;
  object-fit: contain;
}
.preview-icon {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 2rem;
}
.meta-grid {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
.meta-row {
  display: grid;
  grid-template-columns: 100px 1fr;
  align-items: center;
  gap: 1rem;
  font-size: 0.875rem;
}
.meta-label {
  color: var(--p-text-muted-color);
  font-weight: 500;
}
</style>
