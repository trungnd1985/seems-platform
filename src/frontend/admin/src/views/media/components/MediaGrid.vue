<script setup lang="ts">
import type { MediaItem } from '@/types/media'

defineProps<{
  items: MediaItem[]
}>()

const emit = defineEmits<{
  preview: [item: MediaItem]
}>()

function isImage(mimeType: string) {
  return mimeType.startsWith('image/')
}

function isVideo(mimeType: string) {
  return mimeType.startsWith('video/')
}

function fileIcon(mimeType: string): string {
  if (mimeType.includes('pdf')) return 'pi pi-file-pdf'
  if (mimeType.includes('word') || mimeType.includes('document')) return 'pi pi-file-word'
  if (mimeType.includes('zip') || mimeType.includes('compressed')) return 'pi pi-file-arrow-up'
  if (isVideo(mimeType)) return 'pi pi-video'
  return 'pi pi-file'
}

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}
</script>

<template>
  <div v-if="items.length === 0" class="media-grid__empty">
    <i class="pi pi-images text-4xl text-muted-color" />
    <p class="text-muted-color mt-2">No files here</p>
  </div>

  <div v-else class="media-grid">
    <div
      v-for="item in items"
      :key="item.id"
      class="media-grid__card"
      @click="emit('preview', item)"
    >
      <div class="media-grid__thumb">
        <img v-if="isImage(item.mimeType)" :src="item.url" :alt="item.altText || item.originalName" />
        <div v-else class="media-grid__icon">
          <i :class="fileIcon(item.mimeType)" />
        </div>
      </div>
      <div class="media-grid__info">
        <p class="media-grid__name" :title="item.originalName">{{ item.originalName }}</p>
        <p class="media-grid__size">{{ formatSize(item.size) }}</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.media-grid__empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4rem 0;
}

.media-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
  gap: 1rem;
}

.media-grid__card {
  border: 1px solid var(--p-content-border-color);
  border-radius: var(--p-border-radius-md);
  overflow: hidden;
  cursor: pointer;
  transition: box-shadow 0.15s;
}
.media-grid__card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.12);
}

.media-grid__thumb {
  width: 100%;
  aspect-ratio: 1;
  overflow: hidden;
  background: var(--p-content-hover-background);
  display: flex;
  align-items: center;
  justify-content: center;
}
.media-grid__thumb img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
.media-grid__icon {
  font-size: 2.5rem;
  color: var(--p-text-muted-color);
}

.media-grid__info {
  padding: 0.5rem;
}
.media-grid__name {
  font-size: 0.75rem;
  font-weight: 500;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  margin: 0;
  color: var(--p-text-color);
}
.media-grid__size {
  font-size: 0.7rem;
  color: var(--p-text-muted-color);
  margin: 0.15rem 0 0;
}
</style>
