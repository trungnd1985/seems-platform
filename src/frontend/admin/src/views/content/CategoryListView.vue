<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import Button from 'primevue/button'
import Select from 'primevue/select'
import Dialog from 'primevue/dialog'
import Tree from 'primevue/tree'
import { useToast } from 'primevue/usetoast'
import { useCategories } from '@/composables/useCategories'
import CategoryFormDialog from '@/views/content/components/CategoryFormDialog.vue'
import type { Category } from '@/types/categoryTypes'
import type { ContentType } from '@/types/contentTypes'
import { useApi } from '@/composables/useApi'

const api = useApi()
const toast = useToast()
const { categories, loading, fetchCategories, createCategory, updateCategory, deleteCategory } = useCategories()

// ── Content type filter ───────────────────────────────────────────────────
const contentTypes = ref<ContentType[]>([])
const selectedContentTypeKey = ref<string>('')

async function loadContentTypes() {
  const { data } = await api.get<ContentType[]>('/content-types')
  contentTypes.value = data
  if (data.length > 0 && !selectedContentTypeKey.value) {
    selectedContentTypeKey.value = data[0].key
  }
}

watch(selectedContentTypeKey, async (key) => {
  if (key) await fetchCategories(key)
}, { immediate: false })

loadContentTypes().then(() => {
  if (selectedContentTypeKey.value) fetchCategories(selectedContentTypeKey.value)
})

// ── Tree building ─────────────────────────────────────────────────────────
type TreeNode = {
  key: string
  label: string
  data: Category
  leaf: boolean
  children?: TreeNode[]
}

function buildTree(parentId: string | null = null): TreeNode[] {
  return categories.value
    .filter((c) => c.parentId === parentId)
    .sort((a, b) => a.sortOrder - b.sortOrder || a.name.localeCompare(b.name))
    .map((c) => {
      const children = buildTree(c.id)
      return {
        key: c.id,
        label: c.name,
        data: c,
        leaf: children.length === 0,
        children: children.length > 0 ? children : undefined,
      }
    })
}

const treeNodes = computed(() => buildTree(null))
const expandedKeys = ref<Record<string, boolean>>({})

function expandAll() {
  const keys: Record<string, boolean> = {}
  function walk(nodes: TreeNode[]) {
    for (const n of nodes) {
      keys[n.key] = true
      if (n.children) walk(n.children)
    }
  }
  walk(treeNodes.value)
  expandedKeys.value = keys
}

// ── Form dialog ───────────────────────────────────────────────────────────
const formVisible = ref(false)
const editingCategory = ref<Category | null>(null)
const defaultParentId = ref<string | null>(null)

function openCreate(parentId: string | null = null) {
  editingCategory.value = null
  defaultParentId.value = parentId
  formVisible.value = true
}

function openEdit(cat: Category) {
  editingCategory.value = cat
  defaultParentId.value = null
  formVisible.value = true
}

async function onFormSaved(payload: {
  name: string; slug: string; description: string; parentId: string | null; sortOrder: number
}) {
  try {
    if (editingCategory.value) {
      await updateCategory(editingCategory.value.id, {
        name: payload.name,
        slug: payload.slug,
        description: payload.description,
        parentId: payload.parentId,
        sortOrder: payload.sortOrder,
      })
      toast.add({ severity: 'success', summary: 'Category updated', life: 3000 })
    } else {
      await createCategory({
        name: payload.name,
        slug: payload.slug,
        description: payload.description,
        contentTypeKey: selectedContentTypeKey.value,
        parentId: payload.parentId,
        sortOrder: payload.sortOrder,
      })
      toast.add({ severity: 'success', summary: 'Category created', life: 3000 })
    }
    formVisible.value = false
    await fetchCategories(selectedContentTypeKey.value)
  } catch (e: any) {
    const msg = e.response?.data?.message ?? 'Failed to save category.'
    toast.add({ severity: 'error', summary: 'Error', detail: msg, life: 5000 })
  }
}

// ── Delete ────────────────────────────────────────────────────────────────
const deleteVisible = ref(false)
const toDelete = ref<Category | null>(null)
const deleting = ref(false)

function confirmDelete(cat: Category) {
  toDelete.value = cat
  deleteVisible.value = true
}

async function doDelete() {
  if (!toDelete.value) return
  deleting.value = true
  try {
    await deleteCategory(toDelete.value.id)
    deleteVisible.value = false
    toDelete.value = null
    toast.add({ severity: 'success', summary: 'Category deleted', life: 3000 })
    await fetchCategories(selectedContentTypeKey.value)
  } catch (e: any) {
    const msg = e.response?.data?.message ?? 'Failed to delete category.'
    toast.add({ severity: 'error', summary: 'Error', detail: msg, life: 5000 })
  } finally {
    deleting.value = false
  }
}
</script>

<template>
  <div class="view-container">
    <!-- Header -->
    <div class="view-header">
      <div>
        <h1 class="view-title">Categories</h1>
        <p class="view-subtitle">Manage taxonomy for your content types</p>
      </div>
    </div>

    <!-- Toolbar -->
    <div class="toolbar">
      <div class="toolbar-left">
        <label class="toolbar-label">Content Type</label>
        <Select
          :model-value="selectedContentTypeKey"
          @update:model-value="selectedContentTypeKey = $event as string"
          :options="contentTypes"
          option-label="name"
          option-value="key"
          placeholder="Select content type..."
          style="min-width: 220px"
        />
      </div>
      <div class="toolbar-right">
        <Button
          v-if="treeNodes.length > 0"
          label="Expand All"
          icon="pi pi-sitemap"
          text
          size="small"
          @click="expandAll"
        />
        <Button
          label="New Category"
          icon="pi pi-plus"
          :disabled="!selectedContentTypeKey"
          @click="openCreate(null)"
        />
      </div>
    </div>

    <!-- Empty / loading / tree -->
    <div class="tree-panel">
      <div v-if="!selectedContentTypeKey" class="empty-state">
        <i class="pi pi-tag" />
        <p>Select a content type to manage its categories.</p>
      </div>

      <div v-else-if="loading" class="empty-state">
        <i class="pi pi-spin pi-spinner" />
        <p>Loading…</p>
      </div>

      <div v-else-if="treeNodes.length === 0" class="empty-state">
        <i class="pi pi-tag" />
        <p>No categories yet. Click <strong>New Category</strong> to create one.</p>
      </div>

      <Tree
        v-else
        :value="treeNodes"
        :expanded-keys="expandedKeys"
        @update:expanded-keys="expandedKeys = $event"
        class="cat-tree"
      >
        <template #default="{ node }">
          <div class="cat-node">
            <span class="cat-node__icon">
              <i class="pi pi-tag" />
            </span>
            <span class="cat-node__name" :title="node.label">{{ node.label }}</span>
            <span v-if="(node.data as Category).slug" class="cat-node__slug">
              /{{ (node.data as Category).slug }}
            </span>
            <span v-if="(node.data as Category).itemCount > 0" class="cat-node__count">
              {{ (node.data as Category).itemCount }}
            </span>
            <div class="cat-node__actions">
              <Button
                icon="pi pi-plus"
                v-tooltip.top="'Add subcategory'"
                text
                rounded
                size="small"
                @click.stop="openCreate(node.key)"
              />
              <Button
                icon="pi pi-pencil"
                v-tooltip.top="'Edit'"
                text
                rounded
                size="small"
                @click.stop="openEdit(node.data as Category)"
              />
              <Button
                icon="pi pi-trash"
                v-tooltip.top="'Delete'"
                text
                rounded
                size="small"
                severity="danger"
                @click.stop="confirmDelete(node.data as Category)"
              />
            </div>
          </div>
        </template>
      </Tree>
    </div>
  </div>

  <!-- Category form dialog -->
  <CategoryFormDialog
    :visible="formVisible"
    @update:visible="formVisible = $event"
    :category="editingCategory"
    :content-type-key="selectedContentTypeKey"
    :all-categories="categories"
    :default-parent-id="defaultParentId"
    @saved="onFormSaved"
  />

  <!-- Delete confirmation -->
  <Dialog
    :visible="deleteVisible"
    @update:visible="deleteVisible = $event"
    header="Delete Category"
    :style="{ width: '420px' }"
    modal
  >
    <p v-if="toDelete">
      Delete <strong>{{ toDelete.name }}</strong>?
      This cannot be undone. Categories with sub-categories cannot be deleted.
    </p>
    <template #footer>
      <Button label="Cancel" text :disabled="deleting" @click="deleteVisible = false" />
      <Button
        label="Delete"
        severity="danger"
        :loading="deleting"
        @click="doDelete"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.view-container {
  padding: 1.5rem 2rem;
  max-width: 900px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.view-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

.view-title {
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0 0 0.2rem;
}

.view-subtitle {
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
  margin: 0;
}

.toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
}

.toolbar-left {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.toolbar-right {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.toolbar-label {
  font-size: 0.875rem;
  font-weight: 500;
  white-space: nowrap;
}

/* Tree panel */
.tree-panel {
  background: var(--p-content-background);
  border: 1px solid var(--p-content-border-color);
  border-radius: 10px;
  padding: 0.75rem;
  min-height: 200px;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  padding: 3rem;
  color: var(--p-text-muted-color);
  text-align: center;
}

.empty-state i {
  font-size: 2rem;
}

.empty-state p {
  margin: 0;
  font-size: 0.875rem;
}

/* PrimeVue Tree overrides */
.cat-tree :deep(.p-tree) {
  background: transparent;
  border: none;
  padding: 0;
}
.cat-tree :deep(.p-tree-root) {
  padding: 0;
  gap: 2px;
}
.cat-tree :deep(.p-tree-node-content) {
  padding: 0;
  border-radius: 8px;
}
.cat-tree :deep(.p-tree-node-content:hover) {
  background: var(--p-content-hover-background);
}
.cat-tree :deep(.p-tree-node-toggle-button) {
  width: 20px;
  height: 20px;
  margin-left: 2px;
  flex-shrink: 0;
  color: var(--p-text-muted-color);
}
.cat-tree :deep(.p-tree-node-icon) {
  display: none;
}
.cat-tree :deep(.p-tree-children) {
  padding-left: 0.75rem;
  margin-left: 0.5rem;
  border-left: 1.5px solid var(--p-content-border-color);
}

/* Custom node */
.cat-node {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex: 1;
  padding: 0.4rem 0.5rem;
  min-width: 0;
}

.cat-node__icon {
  font-size: 0.85rem;
  color: var(--p-primary-color);
  flex-shrink: 0;
}

.cat-node__name {
  font-size: 0.875rem;
  font-weight: 500;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.cat-node__slug {
  font-size: 0.75rem;
  color: var(--p-text-muted-color);
  font-family: monospace;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  flex-shrink: 0;
}

.cat-node__count {
  font-size: 0.68rem;
  font-weight: 600;
  padding: 0.1rem 0.4rem;
  border-radius: 20px;
  background: var(--p-primary-100, #e8f4fd);
  color: var(--p-primary-color);
  flex-shrink: 0;
}

.cat-node__actions {
  display: none;
  align-items: center;
  gap: 0;
  margin-left: auto;
  flex-shrink: 0;
}

.cat-node:hover .cat-node__actions {
  display: flex;
}
</style>
