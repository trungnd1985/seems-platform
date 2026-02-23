<script setup lang="ts">
import { ref, computed } from 'vue'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import type { ContentField, ContentSchema } from '@/types/contentTypes'
import { FIELD_TYPE_LABELS, FIELD_TYPE_ICONS } from '@/types/contentTypes'
import FieldEditorDialog from './FieldEditorDialog.vue'

const props = defineProps<{
  modelValue: ContentSchema
}>()

const emit = defineEmits<{
  'update:modelValue': [schema: ContentSchema]
}>()

const fields = computed(() => props.modelValue.fields ?? [])

const editorVisible = ref(false)
const editingField = ref<ContentField | null>(null)
const editingIndex = ref(-1)

// Names of all other fields (excludes the one being edited)
const existingNames = computed(() =>
  fields.value.filter((_, i) => i !== editingIndex.value).map((f) => f.name),
)

function openAdd() {
  editingField.value = null
  editingIndex.value = -1
  editorVisible.value = true
}

function openEdit(field: ContentField, index: number) {
  editingField.value = { ...field }
  editingIndex.value = index
  editorVisible.value = true
}

function onFieldSaved(field: ContentField) {
  const updated = [...fields.value]
  if (editingIndex.value >= 0) {
    updated[editingIndex.value] = field
  } else {
    updated.push(field)
  }
  emit('update:modelValue', { fields: updated })
}

function removeField(index: number) {
  emit('update:modelValue', { fields: fields.value.filter((_, i) => i !== index) })
}

function moveUp(index: number) {
  if (index === 0) return
  const updated = [...fields.value]
  ;[updated[index - 1], updated[index]] = [updated[index], updated[index - 1]]
  emit('update:modelValue', { fields: updated })
}

function moveDown(index: number) {
  if (index === fields.value.length - 1) return
  const updated = [...fields.value]
  ;[updated[index], updated[index + 1]] = [updated[index + 1], updated[index]]
  emit('update:modelValue', { fields: updated })
}
</script>

<template>
  <div class="schema-builder">
    <div class="builder-header">
      <span class="builder-title">Fields <span class="count">({{ fields.length }})</span></span>
      <Button label="Add Field" icon="pi pi-plus" size="small" outlined @click="openAdd" />
    </div>

    <div v-if="fields.length === 0" class="empty-state">
      <i class="pi pi-table empty-icon" />
      <p>No fields defined yet.</p>
      <Button label="Add your first field" link size="small" @click="openAdd" />
    </div>

    <div v-else class="field-list">
      <div v-for="(field, index) in fields" :key="field.name" class="field-row">
        <div class="sort-btns">
          <Button
            icon="pi pi-chevron-up"
            text
            rounded
            size="small"
            severity="secondary"
            :disabled="index === 0"
            @click="moveUp(index)"
          />
          <Button
            icon="pi pi-chevron-down"
            text
            rounded
            size="small"
            severity="secondary"
            :disabled="index === fields.length - 1"
            @click="moveDown(index)"
          />
        </div>

        <div class="field-info">
          <i :class="[FIELD_TYPE_ICONS[field.type], 'type-icon']" />
          <div class="field-text">
            <div class="field-label">{{ field.label }}</div>
            <div class="field-meta">
              <code class="field-name">{{ field.name }}</code>
              <Tag :value="FIELD_TYPE_LABELS[field.type]" severity="secondary" />
              <Tag v-if="field.required" value="required" severity="warn" />
              <Tag v-if="field.searchable" value="searchable" severity="info" />
            </div>
          </div>
        </div>

        <div class="field-actions">
          <Button
            icon="pi pi-pencil"
            text
            rounded
            size="small"
            severity="secondary"
            aria-label="Edit field"
            @click="openEdit(field, index)"
          />
          <Button
            icon="pi pi-trash"
            text
            rounded
            size="small"
            severity="danger"
            aria-label="Remove field"
            @click="removeField(index)"
          />
        </div>
      </div>
    </div>

    <FieldEditorDialog
      :visible="editorVisible"
      @update:visible="editorVisible = $event"
      :field="editingField"
      :existing-names="existingNames"
      @saved="onFieldSaved"
    />
  </div>
</template>

<style scoped>
.schema-builder {
  border: 1px solid var(--p-content-border-color);
  border-radius: 8px;
  overflow: hidden;
}

.builder-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.625rem 0.875rem;
  background: var(--p-surface-50);
  border-bottom: 1px solid var(--p-content-border-color);
}

.builder-title {
  font-size: 0.875rem;
  font-weight: 600;
}

.count {
  color: var(--p-text-muted-color);
  font-weight: 400;
}

.empty-state {
  padding: 2rem 1rem;
  text-align: center;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.empty-icon {
  font-size: 1.75rem;
  margin-bottom: 0.5rem;
  display: block;
  opacity: 0.4;
}

.field-list {
  display: flex;
  flex-direction: column;
}

.field-row {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 0.875rem;
  border-bottom: 1px solid var(--p-content-border-color);
  transition: background 0.1s;
}

.field-row:last-child {
  border-bottom: none;
}

.field-row:hover {
  background: var(--p-surface-50);
}

.sort-btns {
  display: flex;
  flex-direction: column;
  gap: 0;
  flex-shrink: 0;
}

.field-info {
  flex: 1;
  display: flex;
  align-items: center;
  gap: 0.625rem;
  min-width: 0;
}

.type-icon {
  font-size: 0.9rem;
  color: var(--p-text-muted-color);
  flex-shrink: 0;
  width: 16px;
}

.field-text {
  min-width: 0;
}

.field-label {
  font-size: 0.875rem;
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.field-meta {
  display: flex;
  align-items: center;
  gap: 0.375rem;
  margin-top: 0.2rem;
}

.field-name {
  font-size: 0.7rem;
  font-family: monospace;
  color: var(--p-text-muted-color);
  background: var(--p-surface-100);
  padding: 0.1rem 0.35rem;
  border-radius: 3px;
}

.field-actions {
  display: flex;
  gap: 0.125rem;
  flex-shrink: 0;
}
</style>
