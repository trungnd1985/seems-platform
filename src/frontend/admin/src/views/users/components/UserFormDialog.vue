<script setup lang="ts">
import { ref, watch } from 'vue'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Checkbox from 'primevue/checkbox'
import Button from 'primevue/button'
import type { User } from '@/types/users'
import { useRoles } from '@/composables/useRoles'

const props = defineProps<{
  visible: boolean
  user: User | null
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  saved: [payload: { email: string; displayName: string; password?: string; roles: string[] }]
}>()

const { roles, fetchRoles } = useRoles()

const email = ref('')
const displayName = ref('')
const password = ref('')
const selectedRoles = ref<string[]>([])
const submitting = ref(false)

watch(
  () => props.visible,
  async (visible) => {
    if (visible) {
      await fetchRoles()
      email.value = props.user?.email ?? ''
      displayName.value = props.user?.displayName ?? ''
      password.value = ''
      selectedRoles.value = props.user ? [...props.user.roles] : []
    }
  },
)

const isEdit = () => props.user !== null

function close() {
  emit('update:visible', false)
}

function submit() {
  submitting.value = true
  try {
    emit('saved', {
      email: email.value.trim(),
      displayName: displayName.value.trim(),
      ...(isEdit() ? {} : { password: password.value }),
      roles: selectedRoles.value,
    })
  } finally {
    submitting.value = false
  }
}

const canSubmit = () => {
  const baseValid = email.value.trim() && displayName.value.trim() && selectedRoles.value.length > 0
  if (!isEdit()) return baseValid && password.value.length >= 8
  return baseValid
}
</script>

<template>
  <Dialog
    :visible="visible"
    :header="isEdit() ? 'Edit User' : 'New User'"
    :style="{ width: '460px' }"
    :closable="!submitting"
    modal
    @update:visible="close"
  >
    <form class="user-form" @submit.prevent="submit">
      <div class="field">
        <label for="u-name">Display Name <span class="required">*</span></label>
        <InputText id="u-name" v-model="displayName" maxlength="100" fluid autofocus />
      </div>

      <div class="field">
        <label for="u-email">Email <span class="required">*</span></label>
        <InputText id="u-email" v-model="email" type="email" maxlength="256" fluid />
      </div>

      <div v-if="!isEdit()" class="field">
        <label for="u-password">Password <span class="required">*</span></label>
        <Password id="u-password" v-model="password" :feedback="false" toggle-mask fluid />
        <small class="hint">Minimum 8 characters.</small>
      </div>

      <div class="field">
        <label>Roles <span class="required">*</span></label>
        <div class="role-list">
          <div v-for="role in roles" :key="role.id" class="role-item">
            <Checkbox
              :inputId="`role-${role.id}`"
              :value="role.name"
              v-model="selectedRoles"
            />
            <label :for="`role-${role.id}`">{{ role.name }}</label>
          </div>
        </div>
      </div>
    </form>

    <template #footer>
      <Button label="Cancel" text :disabled="submitting" @click="close" />
      <Button
        :label="isEdit() ? 'Save Changes' : 'Create User'"
        :loading="submitting"
        :disabled="!canSubmit()"
        @click="submit"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.user-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding: 0.25rem 0;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}

.field label {
  font-size: 0.875rem;
  font-weight: 500;
}

.required {
  color: var(--p-red-500);
}

.hint {
  color: var(--p-text-muted-color);
  font-size: 0.75rem;
}

.role-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding: 0.5rem 0;
}

.role-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
}
</style>
