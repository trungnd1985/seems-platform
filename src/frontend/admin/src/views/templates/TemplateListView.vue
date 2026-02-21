<script setup lang="ts">
import { ref, onMounted } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import Dialog from 'primevue/dialog'
import Toast from 'primevue/toast'
import { useToast } from 'primevue/usetoast'
import { useTemplates } from '@/composables/useTemplates'
import { useThemes } from '@/composables/useThemes'
import TemplateFormDialog from './components/TemplateFormDialog.vue'
import type { Template, TemplateSlotDef } from '@/types/templates'

const toast = useToast()
const { templates, loading, error, fetchTemplates, createTemplate, updateTemplate, deleteTemplate } =
  useTemplates()
const { themes, fetchThemes } = useThemes()

const formDialogVisible = ref(false)
const selectedTemplate = ref<Template | null>(null)

const deleteDialogVisible = ref(false)
const templateToDelete = ref<Template | null>(null)
const deleting = ref(false)

onMounted(async () => {
  await Promise.all([fetchTemplates(), fetchThemes()])
})

function openCreate() {
  selectedTemplate.value = null
  formDialogVisible.value = true
}

function openEdit(template: Template) {
  selectedTemplate.value = template
  formDialogVisible.value = true
}

async function onSaved(payload: {
  key: string
  name: string
  themeKey: string
  slots: TemplateSlotDef[]
}) {
  try {
    if (selectedTemplate.value) {
      await updateTemplate(selectedTemplate.value.id, {
        name: payload.name,
        themeKey: payload.themeKey,
        slots: payload.slots,
      })
      toast.add({ severity: 'success', summary: 'Template updated', life: 3000 })
    } else {
      await createTemplate(payload)
      toast.add({ severity: 'success', summary: 'Template created', life: 3000 })
    }
    formDialogVisible.value = false
    await fetchTemplates()
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: e.response?.data?.message ?? 'Operation failed.',
      life: 5000,
    })
  }
}

function confirmDelete(template: Template) {
  templateToDelete.value = template
  deleteDialogVisible.value = true
}

async function doDelete() {
  if (!templateToDelete.value) return
  deleting.value = true
  try {
    await deleteTemplate(templateToDelete.value.id)
    deleteDialogVisible.value = false
    templateToDelete.value = null
    toast.add({ severity: 'success', summary: 'Template deleted', life: 3000 })
    await fetchTemplates()
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
  templateToDelete.value = null
}
</script>

<template>
  <div class="templates-page">
    <Toast />

    <div class="page-header">
      <div>
        <h1 class="page-title">Templates</h1>
        <p class="page-subtitle">Define page layouts and their named slot structure.</p>
      </div>
      <Button label="New Template" icon="pi pi-plus" @click="openCreate" />
    </div>

    <div v-if="error" class="error-banner">{{ error }}</div>

    <DataTable
      :value="templates"
      :loading="loading"
      striped-rows
      class="templates-table"
    >
      <Column field="key" header="Key" style="width: 200px">
        <template #body="{ data }">
          <code class="key-badge">{{ data.key }}</code>
        </template>
      </Column>

      <Column field="name" header="Name">
        <template #body="{ data }">
          <span class="tpl-name">{{ data.name }}</span>
        </template>
      </Column>

      <Column field="themeKey" header="Theme" style="width: 180px">
        <template #body="{ data }">
          <div class="theme-cell">
            <code class="key-badge">{{ data.themeKey }}</code>
            <Tag
              v-if="!data.themeExists"
              value="missing"
              severity="danger"
              class="ml-1"
            />
          </div>
        </template>
      </Column>

      <Column header="Slots" style="width: 80px; text-align: center">
        <template #body="{ data }">
          <span class="slot-count">{{ data.slots.length }}</span>
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

    <TemplateFormDialog
      :visible="formDialogVisible"
      @update:visible="formDialogVisible = $event"
      :template="selectedTemplate"
      :themes="themes"
      @saved="onSaved"
    />

    <Dialog
      :visible="deleteDialogVisible"
      @update:visible="deleteDialogVisible = $event"
      header="Delete Template"
      :style="{ width: '420px' }"
      :closable="!deleting"
      modal
    >
      <div class="delete-body">
        <i class="pi pi-exclamation-triangle delete-icon" />
        <p>
          Delete template <strong>{{ templateToDelete?.name }}</strong>?
          This will fail if any pages are still using it.
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
.templates-page {
  padding: 1.5rem;
  max-width: 1000px;
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

.tpl-name {
  font-weight: 500;
}

.theme-cell {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.ml-1 {
  margin-left: 0.25rem;
}

.slot-count {
  font-variant-numeric: tabular-nums;
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
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
