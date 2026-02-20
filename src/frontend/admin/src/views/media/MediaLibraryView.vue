<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Paginator from 'primevue/paginator'
import Dialog from 'primevue/dialog'
import { useToast } from 'primevue/usetoast'
import { useMedia } from '@/composables/useMedia'
import type { MediaItem, MediaFolder } from '@/types/media'
import MediaFolderTree from './components/MediaFolderTree.vue'
import MediaGrid from './components/MediaGrid.vue'
import MediaUploadDialog from './components/MediaUploadDialog.vue'
import CreateFolderDialog from './components/CreateFolderDialog.vue'
import RenameFolderDialog from './components/RenameFolderDialog.vue'
import MediaPreviewDialog from './components/MediaPreviewDialog.vue'

const {
  listMedia,
  deleteMedia,
  listFolders,
  deleteFolder,
} = useMedia()
const toast = useToast()

// ── State ─────────────────────────────────────────────────────────────────
const folders = ref<MediaFolder[]>([])
const items = ref<MediaItem[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = 40
const loading = ref(false)

const selectedFolderId = ref<string | null>(null)

// Dialogs
const uploadVisible = ref(false)
const createFolderVisible = ref(false)
const createFolderParentId = ref<string | null>(null)
const renameFolderVisible = ref(false)
const renameFolderTarget = ref<MediaFolder | null>(null)
const deleteFolderVisible = ref(false)
const deleteFolderTarget = ref<MediaFolder | null>(null)
const deleteFolderLoading = ref(false)
const previewVisible = ref(false)
const previewItem = ref<MediaItem | null>(null)

// ── Data loading ──────────────────────────────────────────────────────────
async function fetchFolders() {
  folders.value = await listFolders()
}

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

watch(selectedFolderId, () => {
  page.value = 1
  fetchMedia()
})

watch(page, fetchMedia)

onMounted(async () => {
  await Promise.all([fetchFolders(), fetchMedia()])
})

// ── Folder events ─────────────────────────────────────────────────────────
function onCreateChild(parentId: string) {
  createFolderParentId.value = parentId
  createFolderVisible.value = true
}

function onRenameFolder(folder: MediaFolder) {
  renameFolderTarget.value = folder
  renameFolderVisible.value = true
}

function onDeleteFolderRequest(folder: MediaFolder) {
  deleteFolderTarget.value = folder
  deleteFolderVisible.value = true
}

async function executeDeleteFolder() {
  if (!deleteFolderTarget.value) return
  deleteFolderLoading.value = true
  try {
    await deleteFolder(deleteFolderTarget.value.id)
    deleteFolderVisible.value = false
    // If we were browsing the deleted folder, go back to root
    if (selectedFolderId.value === deleteFolderTarget.value.id) {
      selectedFolderId.value = null
    }
    await Promise.all([fetchFolders(), fetchMedia()])
    toast.add({ severity: 'success', summary: 'Deleted', detail: 'Folder deleted', life: 3000 })
  } catch {
    toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete folder', life: 4000 })
  } finally {
    deleteFolderLoading.value = false
  }
}

// ── Media events ──────────────────────────────────────────────────────────
function onMediaDeleted(id: string) {
  items.value = items.value.filter((i) => i.id !== id)
  total.value = Math.max(0, total.value - 1)
  void deleteMedia(id).catch(() => {
    toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete file', life: 4000 })
    fetchMedia()
  })
}

function openNewFolder() {
  createFolderParentId.value = selectedFolderId.value
  createFolderVisible.value = true
}
</script>

<template>
  <div class="media-library">
    <!-- Sidebar -->
    <aside class="media-library__sidebar">
      <div class="media-library__sidebar-header">
        <div class="sidebar-title">
          <i class="pi pi-folder sidebar-title__icon" />
          <span>Folders</span>
        </div>
        <Button
          icon="pi pi-plus"
          text
          rounded
          size="small"
          severity="secondary"
          v-tooltip.top="'New folder'"
          @click="openNewFolder"
        />
      </div>

      <MediaFolderTree
        :folders="folders"
        :selected-id="selectedFolderId"
        :total-count="total"
        @select="selectedFolderId = $event"
        @rename="onRenameFolder"
        @delete="onDeleteFolderRequest"
        @create-child="onCreateChild"
      />
    </aside>

    <!-- Main -->
    <main class="media-library__main">
      <!-- Toolbar -->
      <div class="media-library__toolbar">
        <Button
          label="Upload"
          icon="pi pi-upload"
          @click="uploadVisible = true"
        />
        <span class="flex-1" />
        <span class="text-sm text-muted-color">{{ total }} file(s)</span>
      </div>

      <!-- Grid -->
      <div v-if="loading" class="flex justify-center py-16">
        <i class="pi pi-spin pi-spinner text-3xl text-muted-color" />
      </div>
      <MediaGrid
        v-else
        :items="items"
        @preview="(item) => { previewItem = item; previewVisible = true }"
      />

      <!-- Paginator -->
      <Paginator
        v-if="total > pageSize"
        :rows="pageSize"
        :total-records="total"
        :first="(page - 1) * pageSize"
        class="mt-4"
        @page="(e) => { page = e.page + 1 }"
      />
    </main>

    <!-- Dialogs — inside single root per ESLint vue/no-multiple-template-root -->
    <MediaUploadDialog
      :visible="uploadVisible"
      @update:visible="uploadVisible = $event"
      :folder-id="selectedFolderId"
      @uploaded="() => fetchMedia()"
    />

    <CreateFolderDialog
      :visible="createFolderVisible"
      @update:visible="createFolderVisible = $event"
      :parent-id="createFolderParentId"
      @created="() => fetchFolders()"
    />

    <RenameFolderDialog
      :visible="renameFolderVisible"
      @update:visible="renameFolderVisible = $event"
      :folder="renameFolderTarget"
      @renamed="() => fetchFolders()"
    />

    <MediaPreviewDialog
      :visible="previewVisible"
      @update:visible="previewVisible = $event"
      :item="previewItem"
      @deleted="onMediaDeleted"
    />

    <!-- Delete folder confirmation — plain Dialog per PrimeVue v4 rules -->
    <Dialog
      :visible="deleteFolderVisible"
      @update:visible="deleteFolderVisible = $event"
      modal
      header="Delete folder"
      :style="{ width: '400px' }"
    >
      <p class="text-sm">
        Delete <strong>{{ deleteFolderTarget?.name }}</strong>? All files and subfolders inside
        will be permanently removed from storage.
      </p>
      <template #footer>
        <Button label="Cancel" severity="secondary" text @click="deleteFolderVisible = false" />
        <Button label="Delete" severity="danger" :loading="deleteFolderLoading" @click="executeDeleteFolder" />
      </template>
    </Dialog>
  </div>
</template>

<style scoped>
.media-library {
  display: flex;
  gap: 0;
  height: calc(100vh - 80px);
  overflow: hidden;
}

/* ── Sidebar ────────────────────────────────────────────────────────────── */
.media-library__sidebar {
  width: 220px;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  padding: 0 0.75rem 1rem 0;
  border-right: 1px solid var(--p-content-border-color);
  overflow-y: auto;
}

.media-library__sidebar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.1rem 0.25rem 0.5rem 0.25rem;
  margin-bottom: 0.1rem;
}

.sidebar-title {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--p-text-muted-color);
}

.sidebar-title__icon {
  font-size: 0.75rem;
}

/* ── Main area ──────────────────────────────────────────────────────────── */
.media-library__main {
  flex: 1;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding-left: 1.25rem;
}

.media-library__toolbar {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding-bottom: 0.75rem;
  border-bottom: 1px solid var(--p-content-border-color);
}
</style>
