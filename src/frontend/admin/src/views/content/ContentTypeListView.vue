<script setup lang="ts">
import { ref, onMounted } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import Dialog from 'primevue/dialog'
import Toast from 'primevue/toast'
import { useToast } from 'primevue/usetoast'
import { useContentTypes } from '@/composables/useContentTypes'
import ContentTypeFormDialog from './components/ContentTypeFormDialog.vue'
import type { ContentType, ContentSchema } from '@/types/contentTypes'

const toast = useToast()
const {
  contentTypes,
  loading,
  error,
  fetchContentTypes,
  createContentType,
  updateContentType,
  deleteContentType,
} = useContentTypes()

const formVisible = ref(false)
const selected = ref<ContentType | null>(null)

const deleteVisible = ref(false)
const toDelete = ref<ContentType | null>(null)
const deleting = ref(false)

onMounted(fetchContentTypes)

function fieldCount(ct: ContentType): number {
  try {
    return (JSON.parse(ct.schema) as ContentSchema).fields?.length ?? 0
  } catch {
    return 0
  }
}

function openCreate() {
  selected.value = null
  formVisible.value = true
}

function openEdit(ct: ContentType) {
  selected.value = ct
  formVisible.value = true
}

async function onSaved(payload: { key: string; name: string; schema: string }) {
  try {
    if (selected.value) {
      await updateContentType(selected.value.id, { name: payload.name, schema: payload.schema })
      toast.add({ severity: 'success', summary: 'Content type updated', life: 3000 })
    } else {
      await createContentType(payload)
      toast.add({ severity: 'success', summary: 'Content type created', life: 3000 })
    }
    formVisible.value = false
    await fetchContentTypes()
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: e.response?.data?.message ?? 'Operation failed.',
      life: 5000,
    })
  }
}

function confirmDelete(ct: ContentType) {
  toDelete.value = ct
  deleteVisible.value = true
}

async function doDelete() {
  if (!toDelete.value) return
  deleting.value = true
  try {
    await deleteContentType(toDelete.value.id)
    deleteVisible.value = false
    toDelete.value = null
    toast.add({ severity: 'success', summary: 'Content type deleted', life: 3000 })
    await fetchContentTypes()
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
  <div class="content-types-page">
    <Toast />

    <div class="page-header">
      <div>
        <h1 class="page-title">Content Types</h1>
        <p class="page-subtitle">Define JSON schemas for your content structures.</p>
      </div>
      <Button label="New Content Type" icon="pi pi-plus" @click="openCreate" />
    </div>

    <div v-if="error" class="error-banner">{{ error }}</div>

    <DataTable :value="contentTypes" :loading="loading" striped-rows>
      <Column field="key" header="Key" style="width: 220px">
        <template #body="{ data }">
          <code class="key-code">{{ data.key }}</code>
        </template>
      </Column>

      <Column field="name" header="Name">
        <template #body="{ data }">
          <span class="ct-name">{{ data.name }}</span>
        </template>
      </Column>

      <Column header="Fields" style="width: 90px; text-align: center">
        <template #body="{ data }">
          <Tag :value="String(fieldCount(data))" severity="secondary" />
        </template>
      </Column>

      <Column header="Actions" style="width: 120px">
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

    <ContentTypeFormDialog
      :visible="formVisible"
      @update:visible="formVisible = $event"
      :content-type="selected"
      @saved="onSaved"
    />

    <Dialog
      :visible="deleteVisible"
      @update:visible="deleteVisible = $event"
      header="Delete Content Type"
      :style="{ width: '430px' }"
      :closable="!deleting"
      modal
    >
      <div class="delete-body">
        <i class="pi pi-exclamation-triangle delete-icon" />
        <p>
          Delete <strong>{{ toDelete?.name }}</strong>
          (<code>{{ toDelete?.key }}</code>)?
          <br />
          <span class="delete-note">All content items of this type must be removed first.</span>
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
.content-types-page {
  padding: 1.5rem;
  max-width: 960px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1.5rem;
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

.ct-name {
  font-weight: 500;
}

.actions {
  display: flex;
  gap: 0.25rem;
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

.delete-note {
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}
</style>
