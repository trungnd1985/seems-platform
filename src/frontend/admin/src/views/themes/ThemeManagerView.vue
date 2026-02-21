<script setup lang="ts">
import { ref, onMounted } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import Toast from 'primevue/toast'
import { useToast } from 'primevue/usetoast'
import { useThemes } from '@/composables/useThemes'
import ThemeFormDialog from './components/ThemeFormDialog.vue'
import type { Theme } from '@/types/themes'

const toast = useToast()
const { themes, loading, error, fetchThemes, createTheme, updateTheme, deleteTheme } = useThemes()

const formDialogVisible = ref(false)
const selectedTheme = ref<Theme | null>(null)

const deleteDialogVisible = ref(false)
const themeToDelete = ref<Theme | null>(null)
const deleting = ref(false)

onMounted(fetchThemes)

function openCreate() {
  selectedTheme.value = null
  formDialogVisible.value = true
}

function openEdit(theme: Theme) {
  selectedTheme.value = theme
  formDialogVisible.value = true
}

async function onSaved(payload: { key: string; name: string; description: string | null; cssUrl: string | null }) {
  try {
    if (selectedTheme.value) {
      await updateTheme(selectedTheme.value.id, { name: payload.name, description: payload.description, cssUrl: payload.cssUrl })
      toast.add({ severity: 'success', summary: 'Theme updated', life: 3000 })
    } else {
      await createTheme(payload)
      toast.add({ severity: 'success', summary: 'Theme created', life: 3000 })
    }
    formDialogVisible.value = false
    await fetchThemes()
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: e.response?.data?.message ?? 'Operation failed.',
      life: 5000,
    })
  }
}

function confirmDelete(theme: Theme) {
  themeToDelete.value = theme
  deleteDialogVisible.value = true
}

async function doDelete() {
  if (!themeToDelete.value) return
  deleting.value = true
  try {
    await deleteTheme(themeToDelete.value.id)
    deleteDialogVisible.value = false
    themeToDelete.value = null
    toast.add({ severity: 'success', summary: 'Theme deleted', life: 3000 })
    await fetchThemes()
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
  deleteDialogVisible.value = false
  themeToDelete.value = null
}
</script>

<template>
  <div class="themes-page">
    <Toast />

    <div class="page-header">
      <div>
        <h1 class="page-title">Themes</h1>
        <p class="page-subtitle">Manage site themes. Each theme groups templates and assets.</p>
      </div>
      <Button label="New Theme" icon="pi pi-plus" @click="openCreate" />
    </div>

    <div v-if="error" class="error-banner">{{ error }}</div>

    <DataTable
      :value="themes"
      :loading="loading"
      striped-rows
      class="themes-table"
    >
      <Column field="key" header="Key" style="width: 200px">
        <template #body="{ data }">
          <code class="key-badge">{{ data.key }}</code>
        </template>
      </Column>

      <Column field="name" header="Name">
        <template #body="{ data }">
          <span class="theme-name">{{ data.name }}</span>
        </template>
      </Column>

      <Column field="description" header="Description">
        <template #body="{ data }">
          <span class="text-muted">{{ data.description ?? '—' }}</span>
        </template>
      </Column>

      <Column field="cssUrl" header="CSS URL">
        <template #body="{ data }">
          <a
            v-if="data.cssUrl"
            :href="data.cssUrl"
            target="_blank"
            rel="noopener noreferrer"
            class="css-url-link"
          >{{ data.cssUrl }}</a>
          <span v-else class="text-muted">—</span>
        </template>
      </Column>

      <Column header="Actions" style="width: 110px">
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

    <ThemeFormDialog
      :visible="formDialogVisible"
      @update:visible="formDialogVisible = $event"
      :theme="selectedTheme"
      @saved="onSaved"
    />

    <Dialog
      :visible="deleteDialogVisible"
      @update:visible="deleteDialogVisible = $event"
      header="Delete Theme"
      :style="{ width: '420px' }"
      :closable="!deleting"
      modal
    >
      <div class="delete-body">
        <i class="pi pi-exclamation-triangle delete-icon" />
        <p>
          Delete theme <strong>{{ themeToDelete?.name }}</strong>?
          This will fail if any templates are still assigned to it.
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
.themes-page {
  padding: 1.5rem;
  max-width: 900px;
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

.key-badge {
  font-family: monospace;
  font-size: 0.8125rem;
  background: var(--p-surface-100);
  border: 1px solid var(--p-surface-200);
  border-radius: 4px;
  padding: 0.125rem 0.375rem;
}

.theme-name {
  font-weight: 500;
}

.text-muted {
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.css-url-link {
  font-size: 0.8125rem;
  color: var(--p-primary-color);
  text-decoration: none;
  max-width: 280px;
  display: inline-block;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  vertical-align: middle;
}

.css-url-link:hover {
  text-decoration: underline;
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
</style>
