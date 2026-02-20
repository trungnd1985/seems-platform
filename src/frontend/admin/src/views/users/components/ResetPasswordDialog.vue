<script setup lang="ts">
import { ref, watch } from 'vue'
import Dialog from 'primevue/dialog'
import Password from 'primevue/password'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { useToast } from 'primevue/usetoast'
import { useUsers } from '@/composables/useUsers'
import type { User } from '@/types/users'

const props = defineProps<{
  visible: boolean
  user: User | null
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  done: []
}>()

const toast = useToast()
const { resetPassword } = useUsers()

const newPassword = ref('')
const submitting = ref(false)
const serverError = ref<string | null>(null)

watch(
  () => props.visible,
  (visible) => {
    if (visible) {
      newPassword.value = ''
      serverError.value = null
    }
  },
)

function close() {
  emit('update:visible', false)
}

async function submit() {
  serverError.value = null
  submitting.value = true
  try {
    await resetPassword(props.user!.id, newPassword.value)
    toast.add({ severity: 'success', summary: 'Password reset successfully', life: 3000 })
    emit('done')
  } catch (e: any) {
    serverError.value = e.response?.data?.message ?? 'Failed to reset password.'
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <Dialog
    :visible="visible"
    :header="`Reset Password — ${user?.displayName}`"
    :style="{ width: '400px' }"
    :closable="!submitting"
    modal
    @update:visible="close"
  >
    <div class="reset-form">
      <Message v-if="serverError" severity="error">{{ serverError }}</Message>

      <div class="field">
        <label for="new-password">New Password <span class="required">*</span></label>
        <Password
          id="new-password"
          v-model="newPassword"
          :feedback="false"
          toggle-mask
          fluid
          autofocus
        />
        <small class="hint">Minimum 8 characters.</small>
      </div>
    </div>

    <template #footer>
      <Button label="Cancel" text :disabled="submitting" @click="close" />
      <Button
        label="Reset Password"
        severity="warning"
        :loading="submitting"
        :disabled="newPassword.length < 8"
        @click="submit"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.reset-form {
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
</style>
