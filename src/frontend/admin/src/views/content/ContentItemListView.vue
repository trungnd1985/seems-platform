<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import Dialog from 'primevue/dialog'
import Select from 'primevue/select'
import Toast from 'primevue/toast'
import Paginator from 'primevue/paginator'
import { useToast } from 'primevue/usetoast'
import { useContentItems } from '@/composables/useContentItems'
import { useContentTypes } from '@/composables/useContentTypes'
import { useCategories } from '@/composables/useCategories'
import ContentItemFormDialog from './components/ContentItemFormDialog.vue'
import type {
  ContentItem,
  ContentType,
  ContentSchema,
  ContentStatus,
} from '@/types/contentTypes'
import {
  CONTENT_STATUSES,
  CONTENT_STATUS_SEVERITY,
} from '@/types/contentTypes'

const toast = useToast()

const {
  items,
  totalCount,
  loading,
  error,
  fetchContentItems,
  createContentItem,
  updateContentItem,
  deleteContentItem,
} = useContentItems()

const { contentTypes, fetchContentTypes } = useContentTypes()
const { categories, fetchCategories } = useCategories()

// Filters
const filterContentTypeKey = ref<string>('')
const filterStatus = ref<ContentStatus | ''>('')
const filterCategoryId = ref<string>('')
const page = ref(1)
const pageSize = 20

// Dialog state
const formVisible = ref(false)
const selected = ref<ContentItem | null>(null)
const selectedContentType = ref<ContentType | null>(null)

const deleteVisible = ref(false)
const toDelete = ref<ContentItem | null>(null)
const deleting = ref(false)

// ContentType picker for new item
const newItemContentTypeKey = ref<string>('')
const pickTypeVisible = ref(false)

const contentTypeOptions = computed(() =>
  contentTypes.value.map((ct) => ({ label: ct.name, value: ct.key })),
)

const statusOptions = [
  { label: 'All Statuses', value: '' },
  ...CONTENT_STATUSES.map((s) => ({ label: s, value: s })),
]

onMounted(async () => {
  await fetchContentTypes()
  await loadItems()
})

watch(filterContentTypeKey, async (key) => {
  filterCategoryId.value = ''
  page.value = 1
  if (key) await fetchCategories(key)
  loadItems()
})

watch(filterStatus, () => {
  page.value = 1
  loadItems()
})

watch(filterCategoryId, () => {
  page.value = 1
  loadItems()
})

async function loadItems() {
  await fetchContentItems({
    page: page.value,
    pageSize,
    contentTypeKey: filterContentTypeKey.value || undefined,
    status: filterStatus.value || undefined,
    categoryId: filterCategoryId.value || undefined,
  })
}

function onPageChange(event: { page: number }) {
  page.value = event.page + 1
  loadItems()
}

function resolveContentType(key: string): ContentType | undefined {
  return contentTypes.value.find((ct) => ct.key === key)
}

function parsedSchema(ct: ContentType | undefined): ContentSchema {
  if (!ct) return { fields: [] }
  try {
    return JSON.parse(ct.schema) as ContentSchema
  } catch {
    return { fields: [] }
  }
}

function dataPreview(item: ContentItem): string {
  const entries = Object.entries(item.data)
    .slice(0, 2)
    .map(([k, v]) => `${k}: ${String(v).slice(0, 40)}`)
    .join(', ')
  return entries || '—'
}

function openCreate() {
  newItemContentTypeKey.value = ''
  pickTypeVisible.value = true
}

function confirmPickType() {
  if (!newItemContentTypeKey.value) return
  const ct = resolveContentType(newItemContentTypeKey.value)
  if (!ct) return
  pickTypeVisible.value = false
  selected.value = null
  selectedContentType.value = ct
  formVisible.value = true
}

function openEdit(item: ContentItem) {
  const ct = resolveContentType(item.contentTypeKey)
  selected.value = item
  selectedContentType.value = ct ?? null
  formVisible.value = true
}

async function onSaved(payload: { data: string; status: ContentStatus; categoryIds: string[] }) {
  try {
    if (selected.value) {
      await updateContentItem(selected.value.id, { data: payload.data, status: payload.status, categoryIds: payload.categoryIds })
      toast.add({ severity: 'success', summary: 'Item updated', life: 3000 })
    } else if (selectedContentType.value) {
      await createContentItem({
        contentTypeKey: selectedContentType.value.key,
        data: payload.data,
        categoryIds: payload.categoryIds,
      })
      toast.add({ severity: 'success', summary: 'Item created', life: 3000 })
    }
    formVisible.value = false
    await loadItems()
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: e.response?.data?.message ?? 'Operation failed.',
      life: 5000,
    })
  }
}

function confirmDelete(item: ContentItem) {
  toDelete.value = item
  deleteVisible.value = true
}

async function doDelete() {
  if (!toDelete.value) return
  deleting.value = true
  try {
    await deleteContentItem(toDelete.value.id)
    deleteVisible.value = false
    toDelete.value = null
    toast.add({ severity: 'success', summary: 'Item deleted', life: 3000 })
    await loadItems()
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: 'Cannot delete',
      detail: e.response?.data?.message ?? 'Delete failed.',
      life: 5000,
    })
  } finally {
    deleting.value = false
  }
}

function cancelDelete() {
  deleteVisible.value = false
  toDelete.value = null
}
</script>

<template>
  <div class="content-items-page">
    <Toast />

    <div class="page-header">
      <div>
        <h1 class="page-title">Content Items</h1>
        <p class="page-subtitle">Create and manage content entries.</p>
      </div>
      <Button label="New Item" icon="pi pi-plus" @click="openCreate" />
    </div>

    <!-- Filters -->
    <div class="filters">
      <Select
        :model-value="filterContentTypeKey"
        @update:model-value="filterContentTypeKey = $event as string"
        :options="[{ label: 'All Types', value: '' }, ...contentTypeOptions]"
        option-label="label"
        option-value="value"
        placeholder="All Types"
        style="width: 220px"
      />
      <Select
        v-if="filterContentTypeKey && categories.length > 0"
        :model-value="filterCategoryId"
        @update:model-value="filterCategoryId = $event as string"
        :options="[{ label: 'All Categories', value: '' }, ...categories.map(c => ({ label: c.name, value: c.id }))]"
        option-label="label"
        option-value="value"
        placeholder="All Categories"
        style="width: 200px"
      />
      <Select
        :model-value="filterStatus"
        @update:model-value="filterStatus = $event as ContentStatus | ''"
        :options="statusOptions"
        option-label="label"
        option-value="value"
        placeholder="All Statuses"
        style="width: 160px"
      />      
    </div>

    <div v-if="error" class="error-banner">{{ error }}</div>

    <DataTable :value="items" :loading="loading" striped-rows>
      <Column header="Content Type" style="width: 200px">
        <template #body="{ data }">
          <code class="key-code">{{ data.contentTypeKey }}</code>
        </template>
      </Column>

      <Column header="Preview">
        <template #body="{ data }">
          <span class="data-preview">{{ dataPreview(data) }}</span>
        </template>
      </Column>

      <Column header="Status" style="width: 110px">
        <template #body="{ data }">
          <Tag
            :value="data.status"
            :severity="CONTENT_STATUS_SEVERITY[data.status as ContentStatus]"
          />
        </template>
      </Column>

      <Column header="Updated" style="width: 160px">
        <template #body="{ data }">
          <span class="date-text">{{ new Date(data.updatedAt).toLocaleString() }}</span>
        </template>
      </Column>

      <Column header="Actions" style="width: 100px">
        <template #body="{ data }">
          <div class="actions">
            <Button
              icon="pi pi-pencil"
              text
              rounded
              severity="secondary"
              aria-label="Edit"
              @click="openEdit(data)"
            />
            <Button
              icon="pi pi-trash"
              text
              rounded
              severity="danger"
              aria-label="Delete"
              @click="confirmDelete(data)"
            />
          </div>
        </template>
      </Column>
    </DataTable>

    <Paginator
      v-if="totalCount > pageSize"
      :rows="pageSize"
      :total-records="totalCount"
      :first="(page - 1) * pageSize"
      @page="onPageChange"
      class="paginator"
    />

    <!-- Content type picker before create -->
    <Dialog
      :visible="pickTypeVisible"
      @update:visible="pickTypeVisible = $event"
      header="Select Content Type"
      :style="{ width: '380px' }"
      modal
    >
      <div class="pick-type-body">
        <p class="pick-type-hint">Choose the content type for the new item.</p>
        <Select
          :model-value="newItemContentTypeKey"
          @update:model-value="newItemContentTypeKey = $event"
          :options="contentTypeOptions"
          option-label="label"
          option-value="value"
          placeholder="Select content type..."
          fluid
        />
      </div>
      <template #footer>
        <Button label="Cancel" text @click="pickTypeVisible = false" />
        <Button
          label="Continue"
          :disabled="!newItemContentTypeKey"
          @click="confirmPickType"
        />
      </template>
    </Dialog>

    <!-- Create / Edit form -->
    <ContentItemFormDialog
      :visible="formVisible"
      @update:visible="formVisible = $event"
      :content-item="selected"
      :schema="parsedSchema(selectedContentType ?? undefined)"
      :content-type-name="selectedContentType?.name ?? ''"
      :content-type-key="selectedContentType?.key ?? ''"
      @saved="onSaved"
    />

    <!-- Delete confirmation -->
    <Dialog
      :visible="deleteVisible"
      @update:visible="deleteVisible = $event"
      header="Delete Content Item"
      :style="{ width: '400px' }"
      :closable="!deleting"
      modal
    >
      <div class="delete-body">
        <i class="pi pi-exclamation-triangle delete-icon" />
        <p>
          Delete this <strong>{{ toDelete?.contentTypeKey }}</strong> item?
          This action cannot be undone.
        </p>
      </div>
      <template #footer>
        <Button label="Cancel" text :disabled="deleting" @click="cancelDelete" />
        <Button label="Delete" severity="danger" :loading="deleting" @click="doDelete" />
      </template>
    </Dialog>
  </div>
</template>

<style scoped>
.content-items-page {
  padding: 1.5rem;
  max-width: 1100px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1.25rem;
}

.page-title {
  font-size: 1.5rem;
  font-weight: 600;
  margin: 0 0 0.25rem;
}

.page-subtitle {
  margin: 0;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.filters {
  display: flex;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.error-banner {
  background: var(--p-red-50);
  color: var(--p-red-700);
  border: 1px solid var(--p-red-200);
  border-radius: 6px;
  padding: 0.75rem 1rem;
  margin-bottom: 1rem;
  font-size: 0.875rem;
}

.key-code {
  font-size: 0.8rem;
  font-family: monospace;
  background: var(--p-surface-100);
  padding: 0.15rem 0.4rem;
  border-radius: 3px;
  color: var(--p-text-muted-color);
}

.data-preview {
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 400px;
  display: inline-block;
}

.date-text {
  font-size: 0.8rem;
  color: var(--p-text-muted-color);
}

.actions {
  display: flex;
  gap: 0.25rem;
}

.paginator {
  margin-top: 1rem;
}

.pick-type-body {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  padding: 0.25rem 0;
}

.pick-type-hint {
  margin: 0;
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.delete-body {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
}

.delete-icon {
  font-size: 1.5rem;
  color: var(--p-yellow-500);
  flex-shrink: 0;
  margin-top: 0.125rem;
}
</style>
