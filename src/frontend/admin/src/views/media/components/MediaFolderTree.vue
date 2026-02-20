<script setup lang="ts">
import { ref, computed } from 'vue'
import Tree from 'primevue/tree'
import Button from 'primevue/button'
import ContextMenu from 'primevue/contextmenu'
import Badge from 'primevue/badge'
import type { MediaFolder } from '@/types/media'

const props = defineProps<{
  folders: MediaFolder[]
  selectedId: string | null
  totalCount?: number
}>()

const emit = defineEmits<{
  select: [folderId: string | null]
  rename: [folder: MediaFolder]
  delete: [folder: MediaFolder]
  createChild: [parentId: string]
}>()

type TreeNode = {
  key: string
  label: string
  data: MediaFolder
  leaf: boolean
  children?: TreeNode[]
}

function buildTree(parentId: string | null = null): TreeNode[] {
  return props.folders
    .filter((f) => f.parentId === parentId)
    .map((f) => {
      const children = buildTree(f.id)
      return {
        key: f.id,
        label: f.name,
        data: f,
        leaf: children.length === 0 && f.childCount === 0,
        children: children.length > 0 ? children : undefined,
      }
    })
}

const treeNodes = computed(() => buildTree(null))

const selectionKeys = computed(() =>
  props.selectedId ? { [props.selectedId]: true } : {},
)
const expandedKeys = ref<Record<string, boolean>>({})

const contextMenu = ref()
const menuFolder = ref<MediaFolder | null>(null)

const menuItems = computed(() => [
  {
    label: 'New subfolder',
    icon: 'pi pi-folder-plus',
    command: () => menuFolder.value && emit('createChild', menuFolder.value.id),
  },
  {
    label: 'Rename',
    icon: 'pi pi-pencil',
    command: () => menuFolder.value && emit('rename', menuFolder.value),
  },
  { separator: true },
  {
    label: 'Delete',
    icon: 'pi pi-trash',
    command: () => menuFolder.value && emit('delete', menuFolder.value),
  },
])

function openMenu(event: MouseEvent, folder: MediaFolder) {
  event.stopPropagation()
  menuFolder.value = folder
  contextMenu.value.show(event)
}

function onNodeSelect(node: TreeNode) {
  emit('select', node.key)
}

function onNodeContextMenu(event: { originalEvent: MouseEvent; node: TreeNode }) {
  menuFolder.value = event.node.data
  contextMenu.value.show(event.originalEvent)
}
</script>

<template>
  <nav class="folder-nav">
    <!-- All media root item -->
    <button
      type="button"
      class="folder-nav__item folder-nav__item--root"
      :class="{ 'folder-nav__item--active': selectedId === null }"
      @click="emit('select', null)"
    >
      <span class="folder-nav__icon folder-nav__icon--root">
        <i class="pi pi-images" />
      </span>
      <span class="folder-nav__label">All media</span>
      <span v-if="totalCount !== undefined" class="folder-nav__count">{{ totalCount }}</span>
    </button>

    <!-- Divider -->
    <div v-if="treeNodes.length" class="folder-nav__divider" />

    <!-- Folder tree -->
    <Tree
      v-if="treeNodes.length"
      :value="treeNodes"
      :selection-keys="selectionKeys"
      :expanded-keys="expandedKeys"
      @update:expanded-keys="expandedKeys = $event"
      selection-mode="single"
      class="folder-tree"
      @node-select="onNodeSelect"
      @node-context-menu-select="onNodeContextMenu"
    >
      <template #default="{ node }">
        <div class="folder-node" @contextmenu.prevent="openMenu($event, node.data)">
          <span class="folder-node__icon">
            <i class="pi" :class="expandedKeys[node.key] ? 'pi-folder-open' : 'pi-folder'" />
          </span>
          <span class="folder-node__label" :title="node.label">{{ node.label }}</span>
          <span v-if="node.data.mediaCount > 0" class="folder-node__badge">
            {{ node.data.mediaCount }}
          </span>
          <button
            type="button"
            class="folder-node__menu-btn"
            @click.stop="openMenu($event, node.data)"
          >
            <i class="pi pi-ellipsis-h" />
          </button>
        </div>
      </template>
    </Tree>

    <p v-else class="folder-nav__empty">
      <i class="pi pi-folder-open" />
      No folders yet
    </p>

    <ContextMenu ref="contextMenu" :model="menuItems" />
  </nav>
</template>

<style scoped>
/* ── Nav container ──────────────────────────────────────────────────────── */
.folder-nav {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

/* ── Shared item base ───────────────────────────────────────────────────── */
.folder-nav__item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  width: 100%;
  padding: 0.45rem 0.6rem;
  border: none;
  border-radius: 8px;
  background: transparent;
  cursor: pointer;
  text-align: left;
  font-size: 0.825rem;
  font-weight: 500;
  color: var(--p-text-color);
  transition: background 0.15s, color 0.15s;
  line-height: 1.4;
}
.folder-nav__item:hover {
  background: var(--p-content-hover-background);
}
.folder-nav__item--active {
  background: var(--p-primary-100, #e8f4fd);
  color: var(--p-primary-600, var(--p-primary-color));
}
.folder-nav__item--active .folder-nav__icon--root {
  color: var(--p-primary-color);
}

/* ── Root item icon ─────────────────────────────────────────────────────── */
.folder-nav__icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 24px;
  border-radius: 6px;
  flex-shrink: 0;
}
.folder-nav__icon--root {
  background: var(--p-primary-100, #e8f4fd);
  color: var(--p-primary-color);
  font-size: 0.8rem;
}

.folder-nav__label {
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.folder-nav__count {
  font-size: 0.7rem;
  font-weight: 600;
  padding: 0.1rem 0.4rem;
  border-radius: 20px;
  background: var(--p-content-hover-background);
  color: var(--p-text-muted-color);
  flex-shrink: 0;
}

/* ── Divider ────────────────────────────────────────────────────────────── */
.folder-nav__divider {
  height: 1px;
  background: var(--p-content-border-color);
  margin: 0.25rem 0.25rem;
  opacity: 0.6;
}

/* ── Empty state ────────────────────────────────────────────────────────── */
.folder-nav__empty {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.8rem;
  color: var(--p-text-muted-color);
  padding: 0.5rem 0.6rem;
  margin: 0;
}

/* ── PrimeVue Tree overrides ────────────────────────────────────────────── */
.folder-tree :deep(.p-tree) {
  background: transparent;
  border: none;
  padding: 0;
}
.folder-tree :deep(.p-tree-root) {
  padding: 0;
  gap: 2px;
}
.folder-tree :deep(.p-tree-node-content) {
  padding: 0;
  border-radius: 8px;
  gap: 0;
}
.folder-tree :deep(.p-tree-node-content:hover) {
  background: var(--p-content-hover-background);
}
.folder-tree :deep(.p-tree-node-content.p-tree-node-selected) {
  background: var(--p-primary-100, #e8f4fd);
  color: var(--p-primary-600, var(--p-primary-color));
}
.folder-tree :deep(.p-tree-node-toggle-button) {
  width: 20px;
  height: 20px;
  margin-left: 2px;
  flex-shrink: 0;
  color: var(--p-text-muted-color);
}
.folder-tree :deep(.p-tree-node-icon) {
  display: none; /* we render our own icon in the slot */
}
.folder-tree :deep(.p-tree-children) {
  padding-left: 0.75rem;
  margin-left: 0.5rem;
  border-left: 1.5px solid var(--p-content-border-color);
}

/* ── Custom node layout ─────────────────────────────────────────────────── */
.folder-node {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  flex: 1;
  padding: 0.35rem 0.4rem 0.35rem 0.25rem;
  min-width: 0;
}

.folder-node__icon {
  font-size: 0.85rem;
  color: var(--p-yellow-500, #f59e0b);
  flex-shrink: 0;
  width: 18px;
  text-align: center;
}
.folder-tree :deep(.p-tree-node-content.p-tree-node-selected) .folder-node__icon {
  color: var(--p-primary-color);
}

.folder-node__label {
  flex: 1;
  font-size: 0.825rem;
  font-weight: 500;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  line-height: 1.4;
}

.folder-node__badge {
  font-size: 0.68rem;
  font-weight: 600;
  padding: 0.1rem 0.35rem;
  border-radius: 20px;
  background: var(--p-content-hover-background);
  color: var(--p-text-muted-color);
  flex-shrink: 0;
  line-height: 1.5;
}

.folder-node__menu-btn {
  display: none;
  align-items: center;
  justify-content: center;
  width: 20px;
  height: 20px;
  border: none;
  border-radius: 4px;
  background: transparent;
  color: var(--p-text-muted-color);
  cursor: pointer;
  padding: 0;
  font-size: 0.7rem;
  flex-shrink: 0;
}
.folder-node__menu-btn:hover {
  background: var(--p-content-border-color);
  color: var(--p-text-color);
}
.folder-node:hover .folder-node__menu-btn {
  display: flex;
}
</style>
