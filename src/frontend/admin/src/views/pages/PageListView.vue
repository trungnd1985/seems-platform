<script setup lang="ts">
import { ref, onMounted } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import Dialog from 'primevue/dialog'
import Toast from 'primevue/toast'
import { useToast } from 'primevue/usetoast'
import { usePages } from '@/composables/usePages'
import { useTemplates } from '@/composables/useTemplates'
import { useThemes } from '@/composables/useThemes'
import PageFormDialog from './components/PageFormDialog.vue'
import type { Page, PageStatus, SeoMeta } from '@/types/pages'
import { PAGE_STATUS_SEVERITIES } from '@/types/pages'

function statusSeverity(status: string): string {
  return PAGE_STATUS_SEVERITIES[status as PageStatus] ?? 'secondary'
}

const toast = useToast()
const { pages, loading, error, fetchPages, createPage, updatePage, deletePage, setDefaultPage } =
  usePages()

const settingDefaultId = ref<string | null>(null)

async function doSetDefault(page: Page) {
  settingDefaultId.value = page.id
  try {
    await setDefaultPage(page.id)
    toast.add({ severity: 'success', summary: `"${page.title}" is now the home page`, life: 3000 })
    await fetchPages()
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: e.response?.data?.message ?? 'Failed to set default page.',
      life: 5000,
    })
  } finally {
    settingDefaultId.value = null
  }
}
const { templates, fetchTemplates } = useTemplates()
const { themes, fetchThemes } = useThemes()

const formDialogVisible = ref(false)
const selectedPage = ref<Page | null>(null)

const deleteDialogVisible = ref(false)
const pageToDelete = ref<Page | null>(null)
const deleting = ref(false)

onMounted(async () => {
  await Promise.all([fetchPages(), fetchTemplates(), fetchThemes()])
})

function openCreate() {
  selectedPage.value = null
  formDialogVisible.value = true
}

function openEdit(page: Page) {
  selectedPage.value = page
  formDialogVisible.value = true
}

async function onSaved(payload: {
  slug: string
  title: string
  templateKey: string
  themeKey: string | null
  seo: SeoMeta
}) {
  try {
    if (selectedPage.value) {
      await updatePage(selectedPage.value.id, { id: selectedPage.value.id, ...payload })
      toast.add({ severity: 'success', summary: 'Page updated', life: 3000 })
    } else {
      await createPage(payload)
      toast.add({ severity: 'success', summary: 'Page created', life: 3000 })
    }
    formDialogVisible.value = false
    await fetchPages()
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: e.response?.data?.message ?? 'Operation failed.',
      life: 5000,
    })
  }
}

function confirmDelete(page: Page) {
  pageToDelete.value = page
  deleteDialogVisible.value = true
}

async function doDelete() {
  if (!pageToDelete.value) return
  deleting.value = true
  try {
    await deletePage(pageToDelete.value.id)
    deleteDialogVisible.value = false
    pageToDelete.value = null
    toast.add({ severity: 'success', summary: 'Page deleted', life: 3000 })
    await fetchPages()
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
  pageToDelete.value = null
}
</script>

<template>
  <div class="pages-page">
    <Toast />

    <div class="page-header">
      <div>
        <h1 class="page-title">Pages</h1>
        <p class="page-subtitle">Manage site pages, their templates, and slot compositions.</p>
      </div>
      <Button label="New Page" icon="pi pi-plus" @click="openCreate" />
    </div>

    <div v-if="error" class="error-banner">{{ error }}</div>

    <DataTable :value="pages" :loading="loading" striped-rows class="pages-table">
      <Column field="slug" header="Slug" style="width: 220px">
        <template #body="{ data }">
          <code class="key-badge">{{ data.slug }}</code>
        </template>
      </Column>

      <Column field="title" header="Title">
        <template #body="{ data }">
          <div class="title-cell">
            <span class="page-name">{{ data.title }}</span>
            <Tag v-if="data.isDefault" value="Home" severity="info" class="home-tag" />
          </div>
        </template>
      </Column>

      <Column field="templateKey" header="Template" style="width: 180px">
        <template #body="{ data }">
          <code class="key-badge">{{ data.templateKey }}</code>
        </template>
      </Column>

      <Column field="status" header="Status" style="width: 120px">
        <template #body="{ data }">
          <Tag :value="data.status" :severity="statusSeverity(data.status)" />
        </template>
      </Column>

      <Column field="updatedAt" header="Updated" style="width: 160px">
        <template #body="{ data }">
          <span class="date-cell">{{ new Date(data.updatedAt).toLocaleDateString() }}</span>
        </template>
      </Column>

      <Column header="Actions" style="width: 150px">
        <template #body="{ data }">
          <div class="actions">
            <Button
              v-if="!data.isDefault"
              icon="pi pi-home"
              text
              rounded
              severity="secondary"
              aria-label="Set as home page"
              v-tooltip.top="'Set as home page'"
              :loading="settingDefaultId === data.id"
              @click="doSetDefault(data)"
            />
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
              :disabled="data.isDefault"
              v-tooltip.top="data.isDefault ? 'Cannot delete the home page' : 'Delete'"
              @click="confirmDelete(data)"
            />
          </div>
        </template>
      </Column>
    </DataTable>

    <PageFormDialog
      :visible="formDialogVisible"
      @update:visible="formDialogVisible = $event"
      :page="selectedPage"
      :templates="templates"
      :themes="themes"
      @saved="onSaved"
    />

    <Dialog
      :visible="deleteDialogVisible"
      @update:visible="deleteDialogVisible = $event"
      header="Delete Page"
      :style="{ width: '420px' }"
      :closable="!deleting"
      modal
    >
      <div class="delete-body">
        <i class="pi pi-exclamation-triangle delete-icon" />
        <p>
          Permanently delete <strong>{{ pageToDelete?.title }}</strong>
          (<code class="key-badge">{{ pageToDelete?.slug }}</code>)?
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
.pages-page {
  padding: 1.5rem;
  max-width: 1100px;
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

.title-cell {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.page-name {
  font-weight: 500;
}

.home-tag {
  font-size: 0.7rem;
}

.date-cell {
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
