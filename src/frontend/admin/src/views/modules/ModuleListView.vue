<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import Dialog from 'primevue/dialog'
import Toast from 'primevue/toast'
import { useToast } from 'primevue/usetoast'
import { useAuthStore } from '@/stores/auth'
import { useModules } from '@/composables/useModules'
import ModuleFormDialog from './components/ModuleFormDialog.vue'
import type { Module, RegisterModuleRequest, UpdateModuleRequest } from '@/types/modules'
import { MODULE_STATUS_SEVERITY } from '@/types/modules'

const router = useRouter()
const toast = useToast()
const auth = useAuthStore()
const isAdmin = computed(() => auth.user?.roles.includes('Admin') ?? false)

function hasSettingsPage(moduleKey: string): boolean {
  return router.resolve({ name: `module-${moduleKey}` }).matched.length > 0
}

const { modules, loading, error, fetchModules, registerModule, updateModule, setStatus, deleteModule } =
  useModules()

const formVisible = ref(false)
const editTarget = ref<Module | null>(null)

const deleteVisible = ref(false)
const toDelete = ref<Module | null>(null)
const deleting = ref(false)

const togglingId = ref<string | null>(null)

onMounted(fetchModules)

function openCreate() {
  editTarget.value = null
  formVisible.value = true
}

function openEdit(mod: Module) {
  editTarget.value = mod
  formVisible.value = true
}

function onFormClose() {
  formVisible.value = false
  editTarget.value = null
}

async function onSaved(payload: RegisterModuleRequest | UpdateModuleRequest) {
  try {
    if (editTarget.value) {
      await updateModule(editTarget.value.id, payload as UpdateModuleRequest)
      formVisible.value = false
      editTarget.value = null
      toast.add({ severity: 'success', summary: 'Module updated', life: 3000 })
    } else {
      await registerModule(payload as RegisterModuleRequest)
      formVisible.value = false
      toast.add({ severity: 'success', summary: 'Module registered', life: 3000 })
    }
    await fetchModules()
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: editTarget.value ? 'Update failed' : 'Registration failed',
      detail: e.response?.data?.message ?? 'Operation failed.',
      life: 5000,
    })
  }
}

async function doToggleStatus(mod: Module) {
  togglingId.value = mod.id
  const next = mod.status === 'Installed' ? 'Disabled' : 'Installed'
  try {
    await setStatus(mod.id, next)
    toast.add({
      severity: 'success',
      summary: next === 'Disabled' ? 'Module disabled' : 'Module enabled',
      life: 3000,
    })
    await fetchModules()
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

function confirmDelete(mod: Module) {
  toDelete.value = mod
  deleteVisible.value = true
}

async function doDelete() {
  if (!toDelete.value) return
  deleting.value = true
  try {
    await deleteModule(toDelete.value.id)
    deleteVisible.value = false
    toDelete.value = null
    toast.add({ severity: 'success', summary: 'Module uninstalled', life: 3000 })
    await fetchModules()
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: 'Cannot uninstall',
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
  <div class="modules-page">
    <Toast />

    <div class="page-header">
      <div>
        <h1 class="page-title">Modules</h1>
        <p class="page-subtitle">Install and manage platform modules.</p>
      </div>
      <Button
        v-if="isAdmin"
        label="Register Module"
        icon="pi pi-plus"
        @click="openCreate"
      />
    </div>

    <div v-if="error" class="error-banner">{{ error }}</div>

    <DataTable :value="modules" :loading="loading" striped-rows>
      <Column field="moduleKey" header="Module Key" style="width: 200px">
        <template #body="{ data }">
          <code class="key-code">{{ data.moduleKey }}</code>
        </template>
      </Column>

      <Column field="name" header="Name">
        <template #body="{ data }">
          <div>
            <span class="mod-name">{{ data.name }}</span>
            <div v-if="data.description" class="mod-desc">{{ data.description }}</div>
          </div>
        </template>
      </Column>

      <Column field="version" header="Version" style="width: 100px">
        <template #body="{ data }">
          <Tag :value="data.version" severity="secondary" />
        </template>
      </Column>

      <Column field="status" header="Status" style="width: 110px">
        <template #body="{ data }">
          <Tag :value="data.status" :severity="MODULE_STATUS_SEVERITY[data.status]" />
        </template>
      </Column>

      <Column header="Component URL" style="width: 220px">
        <template #body="{ data }">
          <a
            v-if="data.publicComponentUrl"
            :href="data.publicComponentUrl"
            target="_blank"
            rel="noopener noreferrer"
            class="url-link"
            :title="data.publicComponentUrl"
          >
            {{ data.publicComponentUrl.replace(/^https?:\/\//, '').slice(0, 40)
            }}{{ data.publicComponentUrl.length > 50 ? '…' : '' }}
          </a>
          <span v-else class="no-url">—</span>
        </template>
      </Column>

      <Column v-if="isAdmin" header="Actions" style="width: 210px">
        <template #body="{ data }">
          <div class="actions">
            <Button
              v-if="hasSettingsPage(data.moduleKey)"
              icon="pi pi-cog"
              text
              rounded
              severity="secondary"
              aria-label="Configure"
              title="Configure"
              @click="router.push({ name: `module-${data.moduleKey}` })"
            />
            <Button
              icon="pi pi-pencil"
              text
              rounded
              severity="secondary"
              aria-label="Edit"
              title="Edit"
              :disabled="togglingId !== null"
              @click="openEdit(data)"
            />
            <Button
              :icon="data.status === 'Installed' ? 'pi pi-pause' : 'pi pi-play'"
              :severity="data.status === 'Installed' ? 'secondary' : 'success'"
              :loading="togglingId === data.id"
              :disabled="togglingId !== null && togglingId !== data.id"
              text
              rounded
              :aria-label="data.status === 'Installed' ? 'Disable' : 'Enable'"
              :title="data.status === 'Installed' ? 'Disable' : 'Enable'"
              @click="doToggleStatus(data)"
            />
            <Button
              icon="pi pi-trash"
              text
              rounded
              severity="danger"
              aria-label="Uninstall"
              title="Uninstall"
              :disabled="togglingId !== null"
              @click="confirmDelete(data)"
            />
          </div>
        </template>
      </Column>
    </DataTable>

    <ModuleFormDialog
      :visible="formVisible"
      :module="editTarget"
      @update:visible="onFormClose"
      @saved="onSaved"
    />

    <Dialog
      :visible="deleteVisible"
      @update:visible="deleteVisible = $event"
      header="Uninstall Module"
      :style="{ width: '430px' }"
      :closable="!deleting"
      modal
    >
      <div class="delete-body">
        <i class="pi pi-exclamation-triangle delete-icon" />
        <p>
          Uninstall <strong>{{ toDelete?.name }}</strong>
          (<code>{{ toDelete?.moduleKey }}</code>)?
          <br />
          <span class="delete-note">
            All page slot assignments for this module will also be removed.
          </span>
        </p>
      </div>

      <template #footer>
        <Button label="Cancel" text :disabled="deleting" @click="cancelDelete" />
        <Button label="Uninstall" severity="danger" :loading="deleting" @click="doDelete" />
      </template>
    </Dialog>
  </div>
</template>

<style scoped>
.modules-page {
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

.key-code {
  font-size: 0.8rem;
  font-family: monospace;
  background: var(--p-surface-100);
  padding: 0.15rem 0.4rem;
  border-radius: 3px;
  color: var(--p-text-muted-color);
}

.mod-name {
  font-weight: 500;
}

.mod-desc {
  font-size: 0.75rem;
  color: var(--p-text-muted-color);
  margin-top: 0.125rem;
}

.url-link {
  font-size: 0.75rem;
  color: var(--p-primary-color);
  text-decoration: none;
  font-family: monospace;
  word-break: break-all;
}

.url-link:hover {
  text-decoration: underline;
}

.no-url {
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
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
