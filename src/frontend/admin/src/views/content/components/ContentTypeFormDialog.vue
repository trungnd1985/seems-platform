<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Button from 'primevue/button'
import Message from 'primevue/message'
import type { ContentType, ContentSchema } from '@/types/contentTypes'
import SchemaBuilder from './SchemaBuilder.vue'

const props = defineProps<{
  visible: boolean
  contentType: ContentType | null
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  saved: [payload: { key: string; name: string; schema: string }]
}>()

const key = ref('')
const name = ref('')
const schema = ref<ContentSchema>({ fields: [] })
const submitting = ref(false)
const serverError = ref<string | null>(null)

const isEdit = computed(() => props.contentType !== null)

watch(
  () => props.contentType,
  (ct) => {
    if (ct) {
      key.value = ct.key
      name.value = ct.name
      try {
        const parsed = JSON.parse(ct.schema) as Partial<ContentSchema>
        schema.value = { fields: Array.isArray(parsed.fields) ? parsed.fields : [] }
      } catch {
        schema.value = { fields: [] }
      }
    } else {
      key.value = ''
      name.value = ''
      schema.value = { fields: [] }
    }
    serverError.value = null
    submitting.value = false
  },
  { immediate: true },
)

const keyError = computed(() => {
  if (!key.value) return null
  if (!/^[a-z][a-z0-9_-]*$/.test(key.value))
    return 'Lowercase only. Start with a letter; letters, digits, hyphens, underscores.'
  return null
})

const canSubmit = computed(
  () => key.value.trim() && !keyError.value && name.value.trim() && !submitting.value,
)

function close() {
  emit('update:visible', false)
}

async function submit() {
  if (!canSubmit.value) return
  serverError.value = null
  submitting.value = true
  try {
    emit('saved', {
      key: key.value.trim(),
      name: name.value.trim(),
      schema: JSON.stringify(schema.value),
    })
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <Dialog
    :visible="visible"
    :header="isEdit ? `Edit: ${contentType?.name}` : 'New Content Type'"
    :style="{ width: '680px', maxHeight: '90vh' }"
    :closable="!submitting"
    modal
    @update:visible="close"
  >
    <div class="ct-form">
      <Message v-if="serverError" severity="error" class="server-error">{{ serverError }}</Message>

      <div class="form-row">
        <div class="field">
          <label for="ct-key">Key <span class="required">*</span></label>
          <InputText
            id="ct-key"
            v-model="key"
            :disabled="isEdit"
            placeholder="e.g. blog_post, product"
            maxlength="128"
            fluid
            autofocus
          />
          <small v-if="keyError && key" class="error-hint">{{ keyError }}</small>
          <small v-else class="hint">Unique API identifier. Immutable after creation.</small>
        </div>

        <div class="field">
          <label for="ct-name">Name <span class="required">*</span></label>
          <InputText
            id="ct-name"
            v-model="name"
            placeholder="e.g. Blog Post, Product"
            maxlength="256"
            fluid
          />
        </div>
      </div>

      <div class="schema-section">
        <SchemaBuilder v-model="schema" />
      </div>
    </div>

    <template #footer>
      <Button label="Cancel" text :disabled="submitting" @click="close" />
      <Button
        :label="isEdit ? 'Save Changes' : 'Create'"
        :loading="submitting"
        :disabled="!canSubmit"
        @click="submit"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.ct-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
  padding: 0.25rem 0;
}

.server-error {
  margin-bottom: 0;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
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

.schema-section {
  /* SchemaBuilder handles its own border/layout */
}

.required {
  color: var(--p-red-500);
}

.hint {
  color: var(--p-text-muted-color);
  font-size: 0.75rem;
}

.error-hint {
  color: var(--p-red-500);
  font-size: 0.75rem;
}
</style>
