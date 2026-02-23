<script setup lang="ts">
import { ref, watch } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import Paginator from 'primevue/paginator'
import InputText from 'primevue/inputtext'
import ProgressBar from 'primevue/progressbar'
import { useMedia } from '@/composables/useMedia'
import type { MediaItem, MediaFolder } from '@/types/media'

const props = defineProps<{
  visible: boolean
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  selected: [item: MediaItem]
}>()

const { listMedia, listFolders, createFolder, uploadMedia } = useMedia()

const folders = ref<MediaFolder[]>([])
const items = ref<MediaItem[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = 30
const loading = ref(false)
const selectedFolderId = ref<string | null>(null)
const pickedItem = ref<MediaItem | null>(null)

// ── Folder creation ───────────────────────────────────────────────────────────
const creatingFolder = ref(false)
const newFolderName = ref('')
const folderSaving = ref(false)

function startCreateFolder() {
  newFolderName.value = ''
  creatingFolder.value = true
}

function cancelCreateFolder() {
  creatingFolder.value = false
}

async function confirmCreateFolder() {
  const name = newFolderName.value.trim()
  if (!name) return
  folderSaving.value = true
  try {
    await createFolder(name, selectedFolderId.value)
    folders.value = await listFolders()
    creatingFolder.value = false
  } finally {
    folderSaving.value = false
  }
}

function onFolderNameKeydown(e: KeyboardEvent) {
  if (e.key === 'Enter') { void confirmCreateFolder() }
  if (e.key === 'Escape') { cancelCreateFolder() }
}
// ─────────────────────────────────────────────────────────────────────────────

// ── File upload ───────────────────────────────────────────────────────────────
const fileInputRef = ref<HTMLInputElement | null>(null)
const uploading = ref(false)
const uploadProgress = ref(0)
const uploadLabel = ref('')

function triggerUpload() {
  fileInputRef.value?.click()
}

async function onFilesSelected(e: Event) {
  const input = e.target as HTMLInputElement
  const files = Array.from(input.files ?? [])
  input.value = '' // reset so same file can be re-uploaded
  if (!files.length) return

  uploading.value = true
  uploadProgress.value = 0

  try {
    for (let i = 0; i < files.length; i++) {
      const file = files[i]
      uploadLabel.value = `Uploading ${i + 1}/${files.length}: ${file.name}`
      await uploadMedia(file, selectedFolderId.value, (pct) => {
        uploadProgress.value = Math.round(((i * 100) + pct) / files.length)
      })
    }
    uploadProgress.value = 100
    await fetchMedia()
  } finally {
    uploading.value = false
    uploadLabel.value = ''
    uploadProgress.value = 0
  }
}
// ─────────────────────────────────────────────────────────────────────────────

watch(
  () => props.visible,
  async (v) => {
    if (!v) return
    pickedItem.value = null
    page.value = 1
    selectedFolderId.value = null
    creatingFolder.value = false
    folders.value = await listFolders()
    await fetchMedia()
  },
)

watch([selectedFolderId, page], fetchMedia)

async function fetchMedia() {
  loading.value = true
  try {
    const result = await listMedia(selectedFolderId.value, page.value, pageSize)
    items.value = result.items
    total.value = result.total
  } finally {
    loading.value = false
  }
}

function onPageChange(event: { page: number }) {
  page.value = event.page + 1
}

function selectFolder(id: string | null) {
  if (selectedFolderId.value === id) return
  selectedFolderId.value = id
  page.value = 1
  creatingFolder.value = false
}

function togglePick(item: MediaItem) {
  pickedItem.value = pickedItem.value?.id === item.id ? null : item
}

function confirm() {
  if (!pickedItem.value) return
  emit('selected', pickedItem.value)
  emit('update:visible', false)
}

function close() {
  emit('update:visible', false)
}

function isImage(mimeType: string) {
  return mimeType.startsWith('image/')
}

function fileIcon(mimeType: string): string {
  if (mimeType.includes('pdf')) return 'pi pi-file-pdf'
  if (mimeType.includes('word') || mimeType.includes('document')) return 'pi pi-file-word'
  if (mimeType.includes('video')) return 'pi pi-video'
  if (mimeType.includes('zip') || mimeType.includes('compressed')) return 'pi pi-file-arrow-up'
  return 'pi pi-file'
}

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}
</script>

<template>
  <Dialog
    :visible="visible"
    header="Select Media"
    :style="{ width: '960px', maxWidth: '96vw' }"
    :closable="true"
    modal
    @update:visible="close"
  >
    <div class="picker-layout">
      <!-- Folder sidebar -->
      <nav class="picker-sidebar">
        <button
          type="button"
          class="folder-btn"
          :class="{ 'folder-btn--active': selectedFolderId === null }"
          @click="selectFolder(null)"
        >
          <i class="pi pi-images" />
          <span>All media</span>
          <span v-if="selectedFolderId === null" class="folder-btn__count">{{ total }}</span>
        </button>

        <div v-if="folders.length" class="folder-divider" />

        <button
          v-for="folder in folders.filter(f => f.parentId === null)"
          :key="folder.id"
          type="button"
          class="folder-btn"
          :class="{ 'folder-btn--active': selectedFolderId === folder.id }"
          @click="selectFolder(folder.id)"
        >
          <i class="pi pi-folder" />
          <span>{{ folder.name }}</span>
          <span class="folder-btn__count">{{ folder.mediaCount }}</span>
        </button>

        <div class="folder-divider" />

        <!-- Inline new-folder form -->
        <div v-if="creatingFolder" class="folder-create">
          <InputText
            v-model="newFolderName"
            size="small"
            placeholder="Folder name"
            class="folder-create__input"
            autofocus
            @keydown="onFolderNameKeydown"
          />
          <div class="folder-create__actions">
            <Button
              icon="pi pi-check"
              size="small"
              text
              :loading="folderSaving"
              :disabled="!newFolderName.trim()"
              @click="() => { void confirmCreateFolder() }"
            />
            <Button
              icon="pi pi-times"
              size="small"
              text
              severity="secondary"
              @click="cancelCreateFolder"
            />
          </div>
        </div>

        <button v-else type="button" class="folder-btn folder-btn--new" @click="startCreateFolder">
          <i class="pi pi-plus" />
          <span>New folder</span>
        </button>
      </nav>

      <!-- Media grid -->
      <div class="picker-main">
        <!-- Upload toolbar -->
        <div class="picker-toolbar">
          <input
            ref="fileInputRef"
            type="file"
            multiple
            style="display: none"
            @change="onFilesSelected"
          />
          <Button
            icon="pi pi-upload"
            label="Upload"
            size="small"
            outlined
            :loading="uploading"
            @click="triggerUpload"
          />
          <div v-if="uploading" class="picker-upload-progress">
            <span class="picker-upload-label">{{ uploadLabel }}</span>
            <ProgressBar :value="uploadProgress" style="height: 6px; flex: 1" />
          </div>
        </div>

        <div v-if="loading" class="picker-loading">
          <i class="pi pi-spin pi-spinner" style="font-size: 1.5rem" />
        </div>

        <div v-else-if="items.length === 0" class="picker-empty">
          <i class="pi pi-images" style="font-size: 2rem" />
          <p>No files here</p>
        </div>

        <div v-else class="picker-grid">
          <div
            v-for="item in items"
            :key="item.id"
            class="picker-card"
            :class="{ 'picker-card--selected': pickedItem?.id === item.id }"
            @click="togglePick(item)"
          >
            <div class="picker-card__check" v-if="pickedItem?.id === item.id">
              <i class="pi pi-check" />
            </div>
            <div class="picker-card__thumb">
              <img
                v-if="isImage(item.mimeType)"
                :src="item.url"
                :alt="item.altText || item.originalName"
              />
              <div v-else class="picker-card__icon">
                <i :class="fileIcon(item.mimeType)" />
              </div>
            </div>
            <div class="picker-card__info">
              <p class="picker-card__name" :title="item.originalName">{{ item.originalName }}</p>
              <p class="picker-card__size">{{ formatSize(item.size) }}</p>
            </div>
          </div>
        </div>

        <Paginator
          v-if="total > pageSize"
          :rows="pageSize"
          :total-records="total"
          :first="(page - 1) * pageSize"
          @page="onPageChange"
          class="picker-paginator"
        />
      </div>
    </div>

    <!-- Selected preview bar -->
    <div v-if="pickedItem" class="picker-selection-bar">
      <img
        v-if="isImage(pickedItem.mimeType)"
        :src="pickedItem.url"
        :alt="pickedItem.originalName"
        class="picker-selection-bar__thumb"
      />
      <i v-else :class="fileIcon(pickedItem.mimeType)" class="picker-selection-bar__icon" />
      <span class="picker-selection-bar__name">{{ pickedItem.originalName }}</span>
      <span class="picker-selection-bar__size">{{ formatSize(pickedItem.size) }}</span>
      <button type="button" class="picker-selection-bar__clear" @click="pickedItem = null">
        <i class="pi pi-times" />
      </button>
    </div>

    <template #footer>
      <Button label="Cancel" text @click="close" />
      <Button label="Choose" :disabled="!pickedItem" @click="confirm" />
    </template>
  </Dialog>
</template>

<style scoped>
.picker-layout {
  display: flex;
  gap: 0;
  height: 480px;
  overflow: hidden;
  border: 1px solid var(--p-content-border-color);
  border-radius: 8px;
}

/* Sidebar */
.picker-sidebar {
  width: 200px;
  flex-shrink: 0;
  border-right: 1px solid var(--p-content-border-color);
  overflow-y: auto;
  padding: 0.5rem;
  display: flex;
  flex-direction: column;
  gap: 2px;
  background: var(--p-content-hover-background);
}

.folder-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  width: 100%;
  padding: 0.4rem 0.6rem;
  border: none;
  border-radius: 6px;
  background: transparent;
  cursor: pointer;
  font-size: 0.82rem;
  font-weight: 500;
  color: var(--p-text-color);
  text-align: left;
  transition: background 0.12s;
}
.folder-btn:hover {
  background: var(--p-content-border-color);
}
.folder-btn--active {
  background: var(--p-primary-100, #e8f4fd);
  color: var(--p-primary-600, var(--p-primary-color));
}
.folder-btn--new {
  color: var(--p-text-muted-color);
  font-weight: 400;
  margin-top: auto;
}
.folder-btn--new:hover {
  color: var(--p-primary-color);
}
.folder-btn span:nth-child(2) {
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.folder-btn__count {
  font-size: 0.7rem;
  font-weight: 600;
  padding: 0.1rem 0.35rem;
  border-radius: 20px;
  background: var(--p-content-hover-background);
  color: var(--p-text-muted-color);
  flex-shrink: 0;
}

.folder-divider {
  height: 1px;
  background: var(--p-content-border-color);
  margin: 0.25rem 0;
}

/* Inline folder creation */
.folder-create {
  padding: 0.35rem 0.25rem;
}
.folder-create__input {
  width: 100%;
  font-size: 0.82rem;
}
.folder-create__actions {
  display: flex;
  justify-content: flex-end;
  gap: 2px;
  margin-top: 2px;
}

/* Main area */
.picker-main {
  flex: 1;
  overflow-y: auto;
  padding: 0.75rem;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

/* Upload toolbar */
.picker-toolbar {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-shrink: 0;
}
.picker-upload-progress {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex: 1;
  min-width: 0;
}
.picker-upload-label {
  font-size: 0.78rem;
  color: var(--p-text-muted-color);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 200px;
}

.picker-loading,
.picker-empty {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: var(--p-text-muted-color);
  gap: 0.5rem;
}
.picker-empty p { margin: 0; font-size: 0.875rem; }

/* Grid */
.picker-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
  gap: 0.75rem;
}

.picker-card {
  position: relative;
  border: 2px solid var(--p-content-border-color);
  border-radius: 8px;
  overflow: hidden;
  cursor: pointer;
  transition: border-color 0.15s, box-shadow 0.15s;
}
.picker-card:hover {
  border-color: var(--p-primary-color);
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}
.picker-card--selected {
  border-color: var(--p-primary-color);
  box-shadow: 0 0 0 2px var(--p-primary-200, #bfdbfe);
}

.picker-card__check {
  position: absolute;
  top: 6px;
  right: 6px;
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background: var(--p-primary-color);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.65rem;
  z-index: 1;
}

.picker-card__thumb {
  width: 100%;
  aspect-ratio: 1;
  overflow: hidden;
  background: var(--p-content-hover-background);
  display: flex;
  align-items: center;
  justify-content: center;
}
.picker-card__thumb img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
.picker-card__icon {
  font-size: 2rem;
  color: var(--p-text-muted-color);
}

.picker-card__info {
  padding: 0.4rem 0.5rem;
  background: var(--p-content-background);
}
.picker-card__name {
  font-size: 0.72rem;
  font-weight: 500;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  margin: 0;
  color: var(--p-text-color);
}
.picker-card__size {
  font-size: 0.68rem;
  color: var(--p-text-muted-color);
  margin: 0.1rem 0 0;
}

.picker-paginator {
  margin-top: auto;
}

/* Selection bar */
.picker-selection-bar {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-top: 0.75rem;
  padding: 0.5rem 0.75rem;
  background: var(--p-primary-50, #eff6ff);
  border: 1px solid var(--p-primary-200, #bfdbfe);
  border-radius: 8px;
  font-size: 0.85rem;
}
.picker-selection-bar__thumb {
  width: 36px;
  height: 36px;
  object-fit: cover;
  border-radius: 4px;
  flex-shrink: 0;
}
.picker-selection-bar__icon {
  font-size: 1.5rem;
  color: var(--p-primary-color);
  flex-shrink: 0;
}
.picker-selection-bar__name {
  flex: 1;
  font-weight: 500;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: var(--p-text-color);
}
.picker-selection-bar__size {
  font-size: 0.75rem;
  color: var(--p-text-muted-color);
  flex-shrink: 0;
}
.picker-selection-bar__clear {
  border: none;
  background: transparent;
  cursor: pointer;
  color: var(--p-text-muted-color);
  padding: 2px;
  border-radius: 4px;
  display: flex;
  align-items: center;
}
.picker-selection-bar__clear:hover {
  color: var(--p-red-500);
}
</style>
