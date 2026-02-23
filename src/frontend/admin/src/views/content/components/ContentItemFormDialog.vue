<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import Message from 'primevue/message'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import InputNumber from 'primevue/inputnumber'
import Checkbox from 'primevue/checkbox'
import DatePicker from 'primevue/datepicker'
import Select from 'primevue/select'
import MultiSelect from 'primevue/multiselect'
import RichTextEditor from '@/components/RichTextEditor.vue'
import MediaPickerDialog from '@/components/MediaPickerDialog.vue'
import type {
  ContentItem,
  ContentSchema,
  ContentField,
  ContentStatus,
} from '@/types/contentTypes'
import { CONTENT_STATUSES } from '@/types/contentTypes'
import type { MediaItem } from '@/types/media'
import { useCategories } from '@/composables/useCategories'

const props = defineProps<{
  visible: boolean
  contentItem: ContentItem | null
  schema: ContentSchema
  contentTypeName: string
  contentTypeKey: string
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  saved: [payload: { data: string; status: ContentStatus; categoryIds: string[] }]
}>()

const formData = ref<Record<string, unknown>>({})
const status = ref<ContentStatus>('Draft')
const selectedCategoryIds = ref<string[]>([])
const submitting = ref(false)
const serverError = ref<string | null>(null)
const validationErrors = ref<Record<string, string>>({})

// Media picker state — one picker for the currently active media field
const mediaPickerVisible = ref(false)
const activeMediaField = ref<ContentField | null>(null)

// Categories
const { categories, fetchCategories } = useCategories()

const isEdit = computed(() => props.contentItem !== null)
const statusOptions = CONTENT_STATUSES.map((s) => ({ label: s, value: s }))

const categoryOptions = computed(() =>
  flattenCategories(categories.value, null, ''),
)

function flattenCategories(
  all: typeof categories.value,
  parentId: string | null,
  prefix: string,
): { label: string; value: string }[] {
  return all
    .filter((c) => c.parentId === parentId)
    .sort((a, b) => a.sortOrder - b.sortOrder || a.name.localeCompare(b.name))
    .flatMap((c) => [
      { label: prefix + c.name, value: c.id },
      ...flattenCategories(all, c.id, prefix + c.name + ' / '),
    ])
}

watch(
  () => [props.contentItem, props.visible] as const,
  ([item, visible]) => {
    if (!visible) return
    serverError.value = null
    validationErrors.value = {}
    submitting.value = false

    if (props.contentTypeKey) {
      void fetchCategories(props.contentTypeKey)
    }

    if (item) {
      formData.value = { ...item.data }
      status.value = item.status
      selectedCategoryIds.value = item.categoryIds ? [...item.categoryIds] : []
    } else {
      formData.value = initFormData(props.schema)
      status.value = 'Draft'
      selectedCategoryIds.value = []
    }
  },
  { immediate: true },
)

function initFormData(schema: ContentSchema): Record<string, unknown> {
  const data: Record<string, unknown> = {}
  for (const field of schema.fields) {
    if (field.type === 'boolean') data[field.name] = false
    else if (field.type === 'number') data[field.name] = null
    else if (field.type === 'select' && field.multiple) data[field.name] = []
    else data[field.name] = ''
  }
  return data
}

function validate(): boolean {
  const errors: Record<string, string> = {}
  for (const field of props.schema.fields) {
    if (!field.required) continue
    const val = formData.value[field.name]
    const empty =
      val === null ||
      val === undefined ||
      val === '' ||
      val === '<p></p>' || // empty Tiptap output
      (Array.isArray(val) && val.length === 0)
    if (empty) errors[field.name] = `${field.label} is required.`
  }
  validationErrors.value = errors
  return Object.keys(errors).length === 0
}

function close() {
  emit('update:visible', false)
}

async function submit() {
  if (!validate()) return
  serverError.value = null
  submitting.value = true
  try {
    emit('saved', {
      data: JSON.stringify(formData.value),
      status: status.value,
      categoryIds: selectedCategoryIds.value,
    })
  } finally {
    submitting.value = false
  }
}

function fieldValue(field: ContentField) {
  return formData.value[field.name]
}

function setFieldValue(field: ContentField, value: unknown) {
  formData.value = { ...formData.value, [field.name]: value }
}

function dateValue(field: ContentField): Date | null {
  const raw = formData.value[field.name]
  if (!raw) return null
  const d = new Date(raw as string)
  return isNaN(d.getTime()) ? null : d
}

function setDateValue(field: ContentField, value: Date | null) {
  formData.value = { ...formData.value, [field.name]: value ? value.toISOString() : '' }
}

function isWideField(field: ContentField): boolean {
  return (
    field.type === 'richtext' ||
    field.type === 'textarea' ||
    (field.type === 'select' && !!field.multiple) ||
    field.type === 'media'
  )
}

// Media field helpers
function openMediaPicker(field: ContentField) {
  activeMediaField.value = field
  mediaPickerVisible.value = true
}

function onMediaSelected(item: MediaItem) {
  if (!activeMediaField.value) return
  // Store full media object so we can show a preview
  setFieldValue(activeMediaField.value, { id: item.id, url: item.url, name: item.originalName, mimeType: item.mimeType })
  activeMediaField.value = null
}

function clearMedia(field: ContentField) {
  setFieldValue(field, '')
}

function mediaFieldValue(field: ContentField): { id: string; url: string; name: string; mimeType: string } | null {
  const val = formData.value[field.name]
  if (val && typeof val === 'object' && 'id' in (val as object)) {
    return val as { id: string; url: string; name: string; mimeType: string }
  }
  return null
}
</script>

<template>
  <Dialog
    :visible="visible"
    :header="isEdit ? `Edit Item — ${contentTypeName}` : `New ${contentTypeName}`"
    :style="{ width: '860px', maxWidth: '96vw' }"
    :closable="!submitting"
    modal
    @update:visible="close"
  >
    <div class="item-form">
      <Message v-if="serverError" severity="error" class="server-error">{{ serverError }}</Message>

      <div v-if="schema.fields.length === 0" class="empty-schema">
        This content type has no fields defined.
      </div>

      <div class="fields-grid">
        <div
          v-for="field in schema.fields"
          :key="field.name"
          class="field"
          :class="{ 'field--wide': isWideField(field) }"
        >
          <label :for="`field-${field.name}`">
            {{ field.label }}
            <span v-if="field.required" class="required">*</span>
          </label>
          <small v-if="field.description" class="hint">{{ field.description }}</small>

          <!-- text -->
          <InputText
            v-if="field.type === 'text'"
            :id="`field-${field.name}`"
            :model-value="(fieldValue(field) as string)"
            @update:model-value="setFieldValue(field, $event)"
            :maxlength="field.maxLength"
            :invalid="!!validationErrors[field.name]"
            fluid
          />

          <!-- textarea (plain) -->
          <Textarea
            v-else-if="field.type === 'textarea'"
            :id="`field-${field.name}`"
            :model-value="(fieldValue(field) as string)"
            @update:model-value="setFieldValue(field, $event)"
            :maxlength="field.maxLength"
            :invalid="!!validationErrors[field.name]"
            rows="4"
            fluid
            auto-resize
          />

          <!-- richtext → WYSIWYG -->
          <RichTextEditor
            v-else-if="field.type === 'richtext'"
            :model-value="(fieldValue(field) as string)"
            @update:model-value="setFieldValue(field, $event)"
            :invalid="!!validationErrors[field.name]"
          />

          <!-- number -->
          <InputNumber
            v-else-if="field.type === 'number'"
            :id="`field-${field.name}`"
            :model-value="(fieldValue(field) as number | null)"
            @update:model-value="setFieldValue(field, $event)"
            :min="field.min"
            :max="field.max"
            :invalid="!!validationErrors[field.name]"
            fluid
          />

          <!-- boolean -->
          <div v-else-if="field.type === 'boolean'" class="boolean-field">
            <Checkbox
              :id="`field-${field.name}`"
              :model-value="(fieldValue(field) as boolean)"
              @update:model-value="setFieldValue(field, $event)"
              binary
            />
            <label :for="`field-${field.name}`" class="boolean-label">{{ field.label }}</label>
          </div>

          <!-- date -->
          <DatePicker
            v-else-if="field.type === 'date'"
            :id="`field-${field.name}`"
            :model-value="dateValue(field)"
            @update:model-value="setDateValue(field, $event as Date | null)"
            :invalid="!!validationErrors[field.name]"
            date-format="yy-mm-dd"
            fluid
          />

          <!-- datetime -->
          <DatePicker
            v-else-if="field.type === 'datetime'"
            :id="`field-${field.name}`"
            :model-value="dateValue(field)"
            @update:model-value="setDateValue(field, $event as Date | null)"
            :invalid="!!validationErrors[field.name]"
            show-time
            hour-format="24"
            fluid
          />

          <!-- select single -->
          <Select
            v-else-if="field.type === 'select' && !field.multiple"
            :id="`field-${field.name}`"
            :model-value="(fieldValue(field) as string)"
            @update:model-value="setFieldValue(field, $event)"
            :options="field.options ?? []"
            :invalid="!!validationErrors[field.name]"
            placeholder="Select..."
            fluid
          />

          <!-- select multiple -->
          <MultiSelect
            v-else-if="field.type === 'select' && field.multiple"
            :id="`field-${field.name}`"
            :model-value="(fieldValue(field) as string[])"
            @update:model-value="setFieldValue(field, $event)"
            :options="field.options ?? []"
            :invalid="!!validationErrors[field.name]"
            placeholder="Select..."
            fluid
          />

          <!-- media → picker dialog -->
          <div v-else-if="field.type === 'media'" class="media-field" :class="{ 'media-field--invalid': !!validationErrors[field.name] }">
            <div v-if="mediaFieldValue(field)" class="media-preview">
              <img
                v-if="mediaFieldValue(field)!.mimeType.startsWith('image/')"
                :src="mediaFieldValue(field)!.url"
                :alt="mediaFieldValue(field)!.name"
                class="media-preview__img"
              />
              <div v-else class="media-preview__icon">
                <i class="pi pi-file" />
              </div>
              <span class="media-preview__name">{{ mediaFieldValue(field)!.name }}</span>
              <Button
                icon="pi pi-times"
                text
                rounded
                severity="secondary"
                size="small"
                aria-label="Remove"
                @click="clearMedia(field)"
              />
            </div>
            <Button
              :label="mediaFieldValue(field) ? 'Change' : 'Choose from library'"
              :icon="mediaFieldValue(field) ? 'pi pi-pencil' : 'pi pi-images'"
              :severity="mediaFieldValue(field) ? 'secondary' : undefined"
              outlined
              size="small"
              @click="openMediaPicker(field)"
            />
          </div>

          <!-- relation: plain text input for ID reference -->
          <InputText
            v-else-if="field.type === 'relation'"
            :id="`field-${field.name}`"
            :model-value="(fieldValue(field) as string)"
            @update:model-value="setFieldValue(field, $event)"
            :invalid="!!validationErrors[field.name]"
            placeholder="Content item ID"
            fluid
          />

          <small v-if="validationErrors[field.name]" class="error-hint">
            {{ validationErrors[field.name] }}
          </small>
        </div>
      </div><!-- /fields-grid -->

      <!-- Categories section -->
      <div v-if="categoryOptions.length > 0" class="categories-section">
        <label class="categories-label">
          <i class="pi pi-tag" />
          Categories
        </label>
        <MultiSelect
          :model-value="selectedCategoryIds"
          @update:model-value="selectedCategoryIds = $event as string[]"
          :options="categoryOptions"
          option-label="label"
          option-value="value"
          placeholder="Assign categories…"
          filter
          display="chip"
          fluid
        />
      </div>

      <div class="status-field">
        <label for="item-status">Status</label>
        <Select
          id="item-status"
          :model-value="status"
          @update:model-value="status = $event as ContentStatus"
          :options="statusOptions"
          option-label="label"
          option-value="value"
          style="width: 200px"
        />
      </div>
    </div>

    <template #footer>
      <Button label="Cancel" text :disabled="submitting" @click="close" />
      <Button
        :label="isEdit ? 'Save Changes' : 'Create'"
        :loading="submitting"
        :disabled="schema.fields.length === 0 && !isEdit"
        @click="submit"
      />
    </template>
  </Dialog>

  <!-- Media picker — rendered outside the main dialog -->
  <MediaPickerDialog
    :visible="mediaPickerVisible"
    @update:visible="mediaPickerVisible = $event"
    @selected="onMediaSelected"
  />
</template>

<style scoped>
.item-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
  padding: 0.25rem 0;
  max-height: calc(90vh - 180px);
  overflow-y: auto;
  padding-right: 0.25rem;
}

.fields-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.25rem 1.5rem;
}

.field--wide {
  grid-column: 1 / -1;
}

.server-error {
  margin-bottom: 0;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}

.field > label {
  font-size: 0.875rem;
  font-weight: 500;
}

.required {
  color: var(--p-red-500);
}

.hint {
  color: var(--p-text-muted-color);
  font-size: 0.75rem;
  margin-top: -0.25rem;
}

.error-hint {
  color: var(--p-red-500);
  font-size: 0.75rem;
}

.boolean-field {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.boolean-label {
  font-size: 0.875rem;
  font-weight: normal;
  cursor: pointer;
}

/* Media field */
.media-field {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.media-field--invalid .media-preview {
  border-color: var(--p-red-400);
}

.media-preview {
  display: flex;
  align-items: center;
  gap: 0.625rem;
  padding: 0.5rem 0.625rem;
  border: 1px solid var(--p-content-border-color);
  border-radius: 6px;
  background: var(--p-content-hover-background);
}

.media-preview__img {
  width: 40px;
  height: 40px;
  object-fit: cover;
  border-radius: 4px;
  flex-shrink: 0;
}

.media-preview__icon {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.4rem;
  color: var(--p-text-muted-color);
  flex-shrink: 0;
}

.media-preview__name {
  flex: 1;
  font-size: 0.85rem;
  font-weight: 500;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* Categories section */
.categories-section {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  border-top: 1px solid var(--p-surface-200);
  padding-top: 1rem;
}

.categories-label {
  font-size: 0.875rem;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 0.375rem;
  color: var(--p-text-color);
}

/* Status row */
.status-field {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  border-top: 1px solid var(--p-surface-200);
  padding-top: 1rem;
}

.status-field label {
  font-size: 0.875rem;
  font-weight: 500;
  white-space: nowrap;
}

.empty-schema {
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
  text-align: center;
  padding: 2rem 0;
}
</style>
