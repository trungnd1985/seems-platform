<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import Dialog from 'primevue/dialog'
import Toast from 'primevue/toast'
import { useToast } from 'primevue/usetoast'
import { useApi } from '@/composables/useApi'
import SlideFormDialog from './components/SlideFormDialog.vue'
import type { SlideRecord, SlideFormPayload } from './components/SlideFormDialog.vue'

const router = useRouter()
const toast = useToast()
const api = useApi()

const slides = ref<SlideRecord[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

const formVisible = ref(false)
const selected = ref<SlideRecord | null>(null)

const deleteVisible = ref(false)
const toDelete = ref<SlideRecord | null>(null)
const deleting = ref(false)

const togglingId = ref<string | null>(null)

async function fetchSlides() {
  loading.value = true
  error.value = null
  try {
    const { data } = await api.get<SlideRecord[]>('/modules/slider/slides/all')
    slides.value = data
  } catch (e: any) {
    error.value = e.response?.data?.message ?? 'Failed to load slides.'
  } finally {
    loading.value = false
  }
}

onMounted(fetchSlides)

function openCreate() {
  selected.value = null
  formVisible.value = true
}

function openEdit(slide: SlideRecord) {
  selected.value = slide
  formVisible.value = true
}

async function onSaved(payload: SlideFormPayload) {
  try {
    if (selected.value) {
      await api.put(`/modules/slider/slides/${selected.value.id}`, payload)
      toast.add({ severity: 'success', summary: 'Slide updated', life: 3000 })
    } else {
      await api.post('/modules/slider/slides', payload)
      toast.add({ severity: 'success', summary: 'Slide created', life: 3000 })
    }
    formVisible.value = false
    await fetchSlides()
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: e.response?.data?.message ?? 'Operation failed.',
      life: 5000,
    })
  }
}

async function togglePublish(slide: SlideRecord) {
  togglingId.value = slide.id
  const nextStatus = slide.status === 'Published' ? 'Draft' : 'Published'
  try {
    await api.put(`/modules/slider/slides/${slide.id}`, {
      title: slide.title,
      subtitle: slide.subtitle,
      imageUrl: slide.imageUrl,
      ctaText: slide.ctaText,
      ctaLink: slide.ctaLink,
      order: slide.order,
      status: nextStatus,
    })
    toast.add({
      severity: 'success',
      summary: nextStatus === 'Published' ? 'Slide published' : 'Slide unpublished',
      life: 3000,
    })
    await fetchSlides()
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: e.response?.data?.message ?? 'Status update failed.',
      life: 5000,
    })
  } finally {
    togglingId.value = null
  }
}

function confirmDelete(slide: SlideRecord) {
  toDelete.value = slide
  deleteVisible.value = true
}

async function doDelete() {
  if (!toDelete.value) return
  deleting.value = true
  try {
    await api.delete(`/modules/slider/slides/${toDelete.value.id}`)
    deleteVisible.value = false
    toDelete.value = null
    toast.add({ severity: 'success', summary: 'Slide deleted', life: 3000 })
    await fetchSlides()
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

const STATUS_SEVERITY: Record<string, string> = {
  Published: 'success',
  Draft: 'secondary',
  Archived: 'warn',
}
</script>

<template>
  <div class="slider-settings-page">
    <Toast />

    <div class="page-header">
      <div class="header-left">
        <Button
          icon="pi pi-arrow-left"
          text
          rounded
          severity="secondary"
          aria-label="Back to Modules"
          @click="router.push({ name: 'modules' })"
        />
        <div>
          <h1 class="page-title">Hero Slider</h1>
          <p class="page-subtitle">Manage slides displayed in the hero section.</p>
        </div>
      </div>
      <Button label="New Slide" icon="pi pi-plus" @click="openCreate" />
    </div>

    <div v-if="error" class="error-banner">{{ error }}</div>

    <DataTable :value="slides" :loading="loading" striped-rows>
      <Column field="order" header="Order" style="width: 80px" sortable>
        <template #body="{ data }">
          <Tag :value="String(data.order)" severity="secondary" />
        </template>
      </Column>

      <Column field="title" header="Title">
        <template #body="{ data }">
          <div>
            <div class="slide-title">{{ data.title }}</div>
            <div v-if="data.subtitle" class="slide-subtitle">{{ data.subtitle }}</div>
          </div>
        </template>
      </Column>

      <Column field="status" header="Status" style="width: 110px">
        <template #body="{ data }">
          <Tag :value="data.status" :severity="STATUS_SEVERITY[data.status] ?? 'secondary'" />
        </template>
      </Column>

      <Column header="CTA" style="width: 160px">
        <template #body="{ data }">
          <span v-if="data.ctaText && data.ctaLink" class="cta-info">
            {{ data.ctaText }} → {{ data.ctaLink }}
          </span>
          <span v-else class="no-cta">—</span>
        </template>
      </Column>

      <Column header="Actions" style="width: 140px">
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
              :icon="data.status === 'Published' ? 'pi pi-eye-slash' : 'pi pi-eye'"
              :severity="data.status === 'Published' ? 'secondary' : 'success'"
              :loading="togglingId === data.id"
              :disabled="togglingId !== null && togglingId !== data.id"
              text
              rounded
              :aria-label="data.status === 'Published' ? 'Unpublish' : 'Publish'"
              :title="data.status === 'Published' ? 'Unpublish' : 'Publish'"
              @click="togglePublish(data)"
            />
            <Button
              icon="pi pi-trash"
              text
              rounded
              severity="danger"
              aria-label="Delete"
              :disabled="togglingId !== null"
              @click="confirmDelete(data)"
            />
          </div>
        </template>
      </Column>
    </DataTable>

    <SlideFormDialog
      :visible="formVisible"
      @update:visible="formVisible = $event"
      :slide="selected"
      @saved="onSaved"
    />

    <Dialog
      :visible="deleteVisible"
      @update:visible="deleteVisible = $event"
      header="Delete Slide"
      :style="{ width: '400px' }"
      :closable="!deleting"
      modal
    >
      <div class="delete-body">
        <i class="pi pi-exclamation-triangle delete-icon" />
        <p>
          Delete slide <strong>{{ toDelete?.title }}</strong>?
          <br />
          <span class="delete-note">This action cannot be undone.</span>
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
.slider-settings-page {
  padding: 1.5rem;
  max-width: 960px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1.5rem;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 0.5rem;
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

.slide-title {
  font-weight: 500;
}

.slide-subtitle {
  font-size: 0.75rem;
  color: var(--p-text-muted-color);
  margin-top: 0.125rem;
}

.cta-info {
  font-size: 0.75rem;
  color: var(--p-text-muted-color);
  font-family: monospace;
}

.no-cta {
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

.delete-note {
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}
</style>
