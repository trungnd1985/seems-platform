<script setup lang="ts">
import { ref, onMounted } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import Dialog from 'primevue/dialog'
import Toast from 'primevue/toast'
import { useToast } from 'primevue/usetoast'
import { useRoles } from '@/composables/useRoles'
import RoleFormDialog from './components/RoleFormDialog.vue'
import type { Role } from '@/types/roles'

const toast = useToast()
const { roles, loading, error, fetchRoles, createRole, updateRole, deleteRole } = useRoles()

const formDialogVisible = ref(false)
const selectedRole = ref<Role | null>(null)

const deleteDialogVisible = ref(false)
const roleToDelete = ref<Role | null>(null)
const deleting = ref(false)

onMounted(fetchRoles)

function openCreate() {
  selectedRole.value = null
  formDialogVisible.value = true
}

function openEdit(role: Role) {
  selectedRole.value = role
  formDialogVisible.value = true
}

async function onSaved(payload: { name: string; description: string | null }) {
  try {
    if (selectedRole.value) {
      await updateRole(selectedRole.value.id, payload)
      toast.add({ severity: 'success', summary: 'Role updated', life: 3000 })
    } else {
      await createRole(payload)
      toast.add({ severity: 'success', summary: 'Role created', life: 3000 })
    }
    formDialogVisible.value = false
    await fetchRoles()
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: e.response?.data?.message ?? 'Operation failed.',
      life: 5000,
    })
  }
}

function confirmDelete(role: Role) {
  roleToDelete.value = role
  deleteDialogVisible.value = true
}

async function doDelete() {
  if (!roleToDelete.value) return
  deleting.value = true
  try {
    await deleteRole(roleToDelete.value.id)
    deleteDialogVisible.value = false
    roleToDelete.value = null
    toast.add({ severity: 'success', summary: 'Role deleted', life: 3000 })
    await fetchRoles()
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
  roleToDelete.value = null
}
</script>

<template>
  <div class="roles-page">
    <Toast />

    <div class="page-header">
      <div>
        <h1 class="page-title">Roles</h1>
        <p class="page-subtitle">Manage access roles for admin users.</p>
      </div>
      <Button label="New Role" icon="pi pi-plus" @click="openCreate" />
    </div>

    <div v-if="error" class="error-banner">{{ error }}</div>

    <DataTable
      :value="roles"
      :loading="loading"
      striped-rows
      class="roles-table"
    >
      <Column field="name" header="Name">
        <template #body="{ data }">
          <span class="role-name">{{ data.name }}</span>
          <Tag v-if="data.isSystem" value="system" severity="secondary" class="ml-2" />
        </template>
      </Column>

      <Column field="description" header="Description">
        <template #body="{ data }">
          <span class="text-muted">{{ data.description ?? '—' }}</span>
        </template>
      </Column>

      <Column field="userCount" header="Users" style="width: 90px; text-align: center" />

      <Column header="Actions" style="width: 130px">
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
              v-if="!data.isSystem"
              icon="pi pi-trash"
              text
              rounded
              severity="danger"
              aria-label="Delete"
              :disabled="data.userCount > 0"
              @click="confirmDelete(data)"
            />
          </div>
        </template>
      </Column>
    </DataTable>

    <RoleFormDialog
      :visible="formDialogVisible"
      @update:visible="formDialogVisible = $event"
      :role="selectedRole"
      @saved="onSaved"
    />

    <Dialog
      :visible="deleteDialogVisible"
      @update:visible="deleteDialogVisible = $event"
      header="Delete Role"
      :style="{ width: '400px' }"
      :closable="!deleting"
      modal
    >
      <div class="delete-body">
        <i class="pi pi-exclamation-triangle delete-icon" />
        <p>
          Delete role <strong>{{ roleToDelete?.name }}</strong>?
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
.roles-page {
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

.role-name {
  font-weight: 500;
}

.text-muted {
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.actions {
  display: flex;
  gap: 0.25rem;
}

.ml-2 {
  margin-left: 0.5rem;
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
