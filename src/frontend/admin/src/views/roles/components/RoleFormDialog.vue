<script setup lang="ts">
import { ref, watch } from 'vue'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Button from 'primevue/button'
import Message from 'primevue/message'
import type { Role } from '@/types/roles'

const props = defineProps<{
  visible: boolean
  role: Role | null
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  saved: [payload: { name: string; description: string | null }]
}>()

const name = ref('')
const description = ref('')
const submitting = ref(false)
const serverError = ref<string | null>(null)

watch(
  () => props.role,
  (r) => {
    name.value = r?.name ?? ''
    description.value = r?.description ?? ''
    serverError.value = null
  },
  { immediate: true },
)

const isSystemRole = () => props.role?.isSystem ?? false
const isEdit = () => props.role !== null

function close() {
  emit('update:visible', false)
}

async function submit() {
  serverError.value = null
  submitting.value = true
  try {
    emit('saved', { name: name.value.trim(), description: description.value.trim() || null })
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <Dialog
    :visible="visible"
    :header="isEdit() ? 'Edit Role' : 'New Role'"
    :style="{ width: '420px' }"
    :closable="!submitting"
    modal
    @update:visible="close"
  >
    <form class="role-form" @submit.prevent="submit">
      <Message v-if="serverError" severity="error" class="mb-3">{{ serverError }}</Message>

      <div class="field">
        <label for="role-name">Name <span class="required">*</span></label>
        <InputText
          id="role-name"
          v-model="name"
          :disabled="isSystemRole()"
          placeholder="e.g. ContentManager"
          maxlength="50"
          fluid
          autofocus
        />
        <small v-if="isSystemRole()" class="hint">System role names cannot be changed.</small>
        <small v-else class="hint">Letters, digits, hyphens, underscores. Must start with a letter.</small>
      </div>

      <div class="field">
        <label for="role-description">Description</label>
        <Textarea
          id="role-description"
          v-model="description"
          placeholder="Describe this role's purpose..."
          rows="3"
          maxlength="256"
          fluid
        />
      </div>
    </form>

    <template #footer>
      <Button label="Cancel" text :disabled="submitting" @click="close" />
      <Button
        :label="isEdit() ? 'Save Changes' : 'Create Role'"
        :loading="submitting"
        :disabled="!name.trim()"
        @click="submit"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.role-form {
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
