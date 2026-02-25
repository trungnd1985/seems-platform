<script setup lang="ts">
import { ref, onMounted } from 'vue'
import TreeTable from 'primevue/treetable'
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
const { pages, treeNodes, loading, error, fetchPageTree, getPage, createPage, updatePage, deletePage, updatePageStatus, setDefaultPage } =
  usePages()

const settingDefaultId = ref<string | null>(null)
const togglingStatusId = ref<string | null>(null)

async function doTogglePublish(page: Page) {
  const next = page.status === 'Published' ? 'Draft' : 'Published'
  togglingStatusId.value = page.id
  try {
    await updatePageStatus(page.id, next)
    toast.add({
      severity: next === 'Published' ? 'success' : 'info',
      summary: next === 'Published' ? `"${page.title}" published` : `"${page.title}" unpublished`,
      life: 3000,
    })
    await fetchPageTree()
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: e.response?.data?.message ?? 'Failed to update status.',
      life: 5000,
    })
  } finally {
    togglingStatusId.value = null
  }
}

async function doSetDefault(page: Page) {
  settingDefaultId.value = page.id
  try {
    await setDefaultPage(page.id)
    toast.add({ severity: 'success', summary: `"${page.title}" is now the home page`, life: 3000 })
    await fetchPageTree()
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
const createParentId = ref<string | null>(null)

const deleteDialogVisible = ref(false)
const pageToDelete = ref<Page | null>(null)
const deleting = ref(false)

onMounted(async () => {
  await Promise.all([fetchPageTree(), fetchTemplates(), fetchThemes()])
})

function openCreate(parentId: string | null = null) {
  selectedPage.value = null
  createParentId.value = parentId
  formDialogVisible.value = true
}

async function openEdit(page: Page) {
  try {
    selectedPage.value = await getPage(page.id)
  } catch {
    selectedPage.value = page
  }
  createParentId.value = null
  formDialogVisible.value = true
}

async function onSaved(payload: {
  slug: string
  title: string
  templateKey: string
  themeKey: string | null
  seo: SeoMeta
  isDefault: boolean
  parentId: string | null
  sortOrder: number
  status: PageStatus
  showInNavigation: boolean
}) {
  try {
    if (selectedPage.value) {
      const { status, ...pagePayload } = payload
      await updatePage(selectedPage.value.id, { id: selectedPage.value.id, ...pagePayload })
      if (status !== selectedPage.value.status) {
        await updatePageStatus(selectedPage.value.id, status)
      }
      toast.add({ severity: 'success', summary: 'Page updated', life: 3000 })
    } else {
      await createPage(payload)
      toast.add({ severity: 'success', summary: 'Page created', life: 3000 })
    }
    formDialogVisible.value = false
    await fetchPageTree()
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
    await fetchPageTree()
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
      <Button label="New Page" icon="pi pi-plus" @click="openCreate()" />
    </div>

    <div v-if="error" class="error-banner">{{ error }}</div>

    <TreeTable :value="treeNodes" :loading="loading" class="pages-table">
      <Column field="title" header="Title" expander style="min-width: 220px">
        <template #body="{ node }">
          <div class="title-cell">
            <span class="page-name">{{ node.data.title }}</span>
            <Tag v-if="node.data.isDefault" value="Home" severity="info" class="home-tag" />
          </div>
        </template>
      </Column>

      <Column header="Path" style="width: 240px">
        <template #body="{ node }">
          <code v-if="node.data.path" class="key-badge">{{ node.data.path }}</code>
          <span v-else class="text-muted">-- (home)</span>
        </template>
      </Column>

      <Column header="Template" style="width: 180px">
        <template #body="{ node }">
          <code class="key-badge">{{ node.data.templateKey }}</code>
        </template>
      </Column>

      <Column header="Status" style="width: 120px">
        <template #body="{ node }">
          <Tag :value="node.data.status" :severity="statusSeverity(node.data.status)" />
        </template>
      </Column>

      <Column header="Nav" style="width: 72px">
        <template #body="{ node }">
          <i
            :class="node.data.showInNavigation ? 'pi pi-eye' : 'pi pi-eye-slash'"
            :style="{ color: node.data.showInNavigation ? 'var(--p-green-500)' : 'var(--p-surface-400)' }"
            v-tooltip.top="node.data.showInNavigation ? 'Visible in navigation' : 'Hidden from navigation'"
          />
        </template>
      </Column>

      <Column header="Updated" style="width: 120px">
        <template #body="{ node }">
          <span class="date-cell">{{ new Date(node.data.updatedAt).toLocaleDateString() }}</span>
        </template>
      </Column>

      <Column header="Actions" style="width: 210px">
        <template #body="{ node }">
          <div class="actions">
            <Button
              :icon="node.data.status === 'Published' ? 'pi pi-eye-slash' : 'pi pi-send'"
              text
              rounded
              :severity="node.data.status === 'Published' ? 'warn' : 'success'"
              :aria-label="node.data.status === 'Published' ? 'Unpublish' : 'Publish'"
              v-tooltip.top="node.data.status === 'Published' ? 'Unpublish' : 'Publish'"
              :loading="togglingStatusId === node.data.id"
              @click="doTogglePublish(node.data)"
            />
            <Button
              v-if="!node.data.isDefault"
              icon="pi pi-home"
              text
              rounded
              severity="secondary"
              aria-label="Set as home page"
              v-tooltip.top="'Set as home page'"
              :loading="settingDefaultId === node.data.id"
              @click="doSetDefault(node.data)"
            />
            <Button
              icon="pi pi-plus"
              text
              rounded
              severity="secondary"
              aria-label="Add child page"
              v-tooltip.top="'Add child page'"
              @click="openCreate(node.data.id)"
            />
            <Button
              icon="pi pi-pencil"
              text
              rounded
              severity="secondary"
              aria-label="Edit"
              @click="openEdit(node.data)"
            />
            <Button
              icon="pi pi-trash"
              text
              rounded
              severity="danger"
              aria-label="Delete"
              :disabled="node.data.isDefault"
              v-tooltip.top="node.data.isDefault ? 'Cannot delete the home page' : 'Delete'"
              @click="confirmDelete(node.data)"
            />
          </div>
        </template>
      </Column>
    </TreeTable>

    <PageFormDialog
      :visible="formDialogVisible"
      @update:visible="formDialogVisible = $event"
      :page="selectedPage"
      :templates="templates"
      :themes="themes"
      :all-pages="pages"
      :initial-parent-id="createParentId"
      @saved="onSaved"
    />

    <Dialog
      :visible="deleteDialogVisible"
      @update:visible="deleteDialogVisible = $event"
      header="Delete Page"
      :style="{ width: '440px' }"
      :closable="!deleting"
      modal
    >
      <div class="delete-body">
        <i class="pi pi-exclamation-triangle delete-icon" />
        <p>
          Permanently delete <strong>{{ pageToDelete?.title }}</strong>
          (<code class="key-badge">{{ pageToDelete?.path }}</code>)?
          <br />
          <span class="hint-warn">Pages with children cannot be deleted -- re-parent or delete children first.</span>
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
  max-width: 1200px;
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

.text-muted {
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
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

.hint-warn {
  color: var(--p-orange-500);
  font-size: 0.8125rem;
}
</style>

