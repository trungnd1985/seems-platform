<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Select from 'primevue/select'
import Checkbox from 'primevue/checkbox'
import Button from 'primevue/button'
import type { ContentField, FieldType } from '@/types/contentTypes'
import { FIELD_TYPES } from '@/types/contentTypes'

const props = defineProps<{
  visible: boolean
  field: ContentField | null
  existingNames: string[]
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  saved: [field: ContentField]
}>()

const name = ref('')
const label = ref('')
const type = ref<FieldType>('text')
const required = ref(false)
const description = ref('')
const multiple = ref(false)
const optionsRaw = ref('')

const isEdit = computed(() => props.field !== null)

const nameError = computed(() => {
  if (!name.value) return null
  if (!/^[a-z][a-z0-9_]*$/.test(name.value))
    return 'Must be lowercase, start with a letter; letters, digits, underscores only.'
  if (!isEdit.value && props.existingNames.includes(name.value))
    return 'A field with this name already exists.'
  return null
})

const canSubmit = computed(
  () => name.value.trim() && label.value.trim() && !nameError.value,
)

watch(
  () => props.field,
  (f) => {
    name.value = f?.name ?? ''
    label.value = f?.label ?? ''
    type.value = f?.type ?? 'text'
    required.value = f?.required ?? false
    description.value = f?.description ?? ''
    multiple.value = f?.multiple ?? false
    optionsRaw.value = f?.options?.join(', ') ?? ''
  },
  { immediate: true },
)

// Auto-populate label from name while creating
let labelTouched = false
watch(label, () => { labelTouched = true })
watch(name, (n) => {
  if (!isEdit.value && !labelTouched) {
    label.value = n.replace(/_/g, ' ').replace(/\b\w/g, (c) => c.toUpperCase())
  }
})

function close() {
  labelTouched = false
  emit('update:visible', false)
}

function submit() {
  if (!canSubmit.value) return

  const field: ContentField = {
    name: name.value,
    label: label.value.trim(),
    type: type.value,
    required: required.value,
  }

  if (description.value.trim()) field.description = description.value.trim()

  if (type.value === 'select') {
    field.multiple = multiple.value
    field.options = optionsRaw.value
      .split(',')
      .map((s) => s.trim())
      .filter(Boolean)
  }

  emit('saved', field)
  close()
}
</script>

<template>
  <Dialog
    :visible="visible"
    :header="isEdit ? 'Edit Field' : 'Add Field'"
    :style="{ width: '480px' }"
    modal
    @update:visible="close"
  >
    <div class="field-form">
      <div class="field">
        <label>Type <span class="required">*</span></label>
        <Select
          v-model="type"
          :options="FIELD_TYPES"
          option-label="label"
          option-value="value"
          placeholder="Select a type"
          :disabled="isEdit"
          fluid
        />
        <small v-if="isEdit" class="hint">Field type cannot be changed after creation.</small>
      </div>

      <div class="field">
        <label>Name <span class="required">*</span></label>
        <InputText
          v-model="name"
          :disabled="isEdit"
          placeholder="e.g. title, body_text"
          maxlength="64"
          fluid
          autofocus
        />
        <small v-if="nameError" class="error-hint">{{ nameError }}</small>
        <small v-else class="hint">API identifier — snake_case. Cannot be changed after creation.</small>
      </div>

      <div class="field">
        <label>Label <span class="required">*</span></label>
        <InputText
          v-model="label"
          placeholder="e.g. Title, Body Text"
          maxlength="128"
          fluid
        />
      </div>

      <div class="field">
        <label>Description</label>
        <Textarea
          v-model="description"
          placeholder="Help text shown in the content editor..."
          rows="2"
          fluid
        />
      </div>

      <div class="field-check">
        <Checkbox v-model="required" binary input-id="field-required" />
        <label for="field-required">Required field</label>
      </div>

      <template v-if="type === 'select'">
        <div class="field-check">
          <Checkbox v-model="multiple" binary input-id="field-multiple" />
          <label for="field-multiple">Allow multiple values</label>
        </div>

        <div class="field">
          <label>Options</label>
          <Textarea
            v-model="optionsRaw"
            placeholder="option1, option2, option3"
            rows="2"
            fluid
          />
          <small class="hint">Comma-separated list of allowed values.</small>
        </div>
      </template>
    </div>

    <template #footer>
      <Button label="Cancel" text @click="close" />
      <Button
        :label="isEdit ? 'Save Changes' : 'Add Field'"
        :disabled="!canSubmit"
        @click="submit"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.field-form {
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

.field-check {
  display: flex;
  align-items: center;
  gap: 0.5rem;
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

.error-hint {
  color: var(--p-red-500);
  font-size: 0.75rem;
}
</style>
