<script setup lang="ts">
import { ref, onMounted } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import Dialog from 'primevue/dialog'
import Toast from 'primevue/toast'
import { useToast } from 'primevue/usetoast'
import { useAuthStore } from '@/stores/auth'
import { useUsers } from '@/composables/useUsers'
import UserFormDialog from './components/UserFormDialog.vue'
import ResetPasswordDialog from './components/ResetPasswordDialog.vue'
import type { User } from '@/types/users'

const toast = useToast()
const auth = useAuthStore()
const { users, total, loading, error, fetchUsers, createUser, updateUser, deleteUser, lockUser, unlockUser } =
  useUsers()

const page = ref(1)
const pageSize = 20

// Form dialog
const formVisible = ref(false)
const selectedUser = ref<User | null>(null)

// Reset password dialog
const resetVisible = ref(false)
const resetTarget = ref<User | null>(null)

// Delete dialog
const deleteVisible = ref(false)
const deleteTarget = ref<User | null>(null)
const deleting = ref(false)

// Lock/Unlock in-progress tracking
const lockingId = ref<string | null>(null)

onMounted(() => fetchUsers(page.value, pageSize))

function openCreate() {
  selectedUser.value = null
  formVisible.value = true
}

function openEdit(user: User) {
  selectedUser.value = user
  formVisible.value = true
}

async function onSaved(payload: { email: string; displayName: string; password?: string; roles: string[] }) {
  try {
    if (selectedUser.value) {
      await updateUser(selectedUser.value.id, {
        email: payload.email,
        displayName: payload.displayName,
        roles: payload.roles,
      })
      toast.add({ severity: 'success', summary: 'User updated', life: 3000 })
    } else {
      await createUser({
        email: payload.email,
        displayName: payload.displayName,
        password: payload.password!,
        roles: payload.roles,
      })
      toast.add({ severity: 'success', summary: 'User created', life: 3000 })
    }
    formVisible.value = false
    await fetchUsers(page.value, pageSize)
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: e.response?.data?.message ?? 'Operation failed.',
      life: 5000,
    })
  }
}

function openResetPassword(user: User) {
  resetTarget.value = user
  resetVisible.value = true
}

function confirmDelete(user: User) {
  deleteTarget.value = user
  deleteVisible.value = true
}

async function doDelete() {
  if (!deleteTarget.value) return
  deleting.value = true
  try {
    await deleteUser(deleteTarget.value.id)
    deleteVisible.value = false
    deleteTarget.value = null
    toast.add({ severity: 'success', summary: 'User deleted', life: 3000 })
    await fetchUsers(page.value, pageSize)
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

async function toggleLock(user: User) {
  lockingId.value = user.id
  try {
    if (user.isLockedOut) {
      await unlockUser(user.id)
      toast.add({ severity: 'success', summary: `${user.displayName} unlocked`, life: 3000 })
    } else {
      await lockUser(user.id)
      toast.add({ severity: 'warn', summary: `${user.displayName} locked`, life: 3000 })
    }
    await fetchUsers(page.value, pageSize)
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: e.response?.data?.message ?? 'Operation failed.',
      life: 5000,
    })
  } finally {
    lockingId.value = null
  }
}

const isSelf = (user: User) => user.id === auth.user?.id.toString()
</script>

<template>
  <div class="users-page">
    <Toast />

    <div class="page-header">
      <div>
        <h1 class="page-title">Users</h1>
        <p class="page-subtitle">Manage admin users and their access.</p>
      </div>
      <Button label="New User" icon="pi pi-plus" @click="openCreate" />
    </div>

    <div v-if="error" class="error-banner">{{ error }}</div>

    <DataTable :value="users" :loading="loading" striped-rows>
      <Column field="displayName" header="Name" />

      <Column field="email" header="Email" />

      <Column header="Roles">
        <template #body="{ data }">
          <div class="role-tags">
            <Tag
              v-for="role in data.roles"
              :key="role"
              :value="role"
              severity="secondary"
              class="role-tag"
            />
          </div>
        </template>
      </Column>

      <Column header="Status" style="width: 110px">
        <template #body="{ data }">
          <Tag
            :value="data.isLockedOut ? 'Locked' : 'Active'"
            :severity="data.isLockedOut ? 'danger' : 'success'"
          />
        </template>
      </Column>

      <Column header="Actions" style="width: 180px">
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
              icon="pi pi-key"
              text
              rounded
              severity="secondary"
              aria-label="Reset password"
              @click="openResetPassword(data)"
            />
            <Button
              :icon="data.isLockedOut ? 'pi pi-lock-open' : 'pi pi-lock'"
              text
              rounded
              :severity="data.isLockedOut ? 'warn' : 'secondary'"
              :loading="lockingId === data.id"
              :disabled="isSelf(data)"
              :aria-label="data.isLockedOut ? 'Unlock' : 'Lock'"
              @click="toggleLock(data)"
            />
            <Button
              icon="pi pi-trash"
              text
              rounded
              severity="danger"
              aria-label="Delete"
              :disabled="isSelf(data)"
              @click="confirmDelete(data)"
            />
          </div>
        </template>
      </Column>
    </DataTable>

    <!-- Create / Edit -->
    <UserFormDialog
      :visible="formVisible"
      @update:visible="formVisible = $event"
      :user="selectedUser"
      @saved="onSaved"
    />

    <!-- Reset Password -->
    <ResetPasswordDialog
      :visible="resetVisible"
      @update:visible="resetVisible = $event"
      :user="resetTarget"
      @done="resetVisible = false"
    />

    <!-- Delete confirm -->
    <Dialog
      :visible="deleteVisible"
      @update:visible="deleteVisible = $event"
      header="Delete User"
      :style="{ width: '400px' }"
      :closable="!deleting"
      modal
    >
      <div class="delete-body">
        <i class="pi pi-exclamation-triangle delete-icon" />
        <p>
          Delete user <strong>{{ deleteTarget?.displayName }}</strong>?
          This action cannot be undone.
        </p>
      </div>
      <template #footer>
        <Button label="Cancel" text :disabled="deleting" @click="deleteVisible = false" />
        <Button label="Delete" severity="danger" :loading="deleting" @click="doDelete" />
      </template>
    </Dialog>
  </div>
</template>

<style scoped>
.users-page {
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

.role-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 0.25rem;
}

.actions {
  display: flex;
  gap: 0.125rem;
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
