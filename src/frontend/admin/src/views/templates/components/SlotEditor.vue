<script setup lang="ts">
import { ref, watch } from 'vue'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Button from 'primevue/button'
import type { TemplateSlotDef } from '@/types/templates'

const props = defineProps<{
  modelValue: TemplateSlotDef[]
}>()

const emit = defineEmits<{
  'update:modelValue': [value: TemplateSlotDef[]]
}>()

const slots = ref<TemplateSlotDef[]>([])

watch(
  () => props.modelValue,
  (val) => {
    slots.value = val.map((s) => ({ ...s }))
  },
  { immediate: true, deep: true },
)

function emitUpdate() {
  emit('update:modelValue', slots.value.map((s) => ({ ...s })))
}

function addSlot() {
  slots.value.push({ key: '', label: '', maxItems: null })
  emitUpdate()
}

function removeSlot(index: number) {
  slots.value.splice(index, 1)
  emitUpdate()
}

function onFieldChange() {
  emitUpdate()
}

function isDuplicateKey(index: number): boolean {
  const key = slots.value[index].key.trim()
  if (!key) return false
  return slots.value.some((s, i) => i !== index && s.key.trim() === key)
}

function isInvalidKey(key: string): boolean {
  return key.length > 0 && !/^[a-z][a-z0-9_-]*$/.test(key)
}
</script>

<template>
  <div class="slot-editor">
    <div class="slot-editor-header">
      <span class="slot-editor-label">Slots</span>
      <Button
        label="Add Slot"
        icon="pi pi-plus"
        size="small"
        text
        @click="addSlot"
      />
    </div>

    <div v-if="slots.length === 0" class="slot-empty">
      No slots defined. Add at least one slot for this template.
    </div>

    <div v-else class="slot-list">
      <div class="slot-list-header">
        <span class="col-key">Key</span>
        <span class="col-label">Label</span>
        <span class="col-max">Max Items</span>
        <span class="col-action" />
      </div>

      <div
        v-for="(slot, index) in slots"
        :key="index"
        class="slot-row"
      >
        <div class="col-key">
          <InputText
            v-model="slot.key"
            placeholder="e.g. hero"
            maxlength="64"
            :invalid="isDuplicateKey(index) || isInvalidKey(slot.key)"
            size="small"
            fluid
            @update:modelValue="onFieldChange"
          />
          <small v-if="isDuplicateKey(index)" class="slot-error">Duplicate key</small>
          <small v-else-if="isInvalidKey(slot.key)" class="slot-error">Invalid format</small>
        </div>

        <div class="col-label">
          <InputText
            v-model="slot.label"
            placeholder="e.g. Hero Section"
            maxlength="128"
            size="small"
            fluid
            @update:modelValue="onFieldChange"
          />
        </div>

        <div class="col-max">
          <InputNumber
            v-model="slot.maxItems"
            placeholder="∞"
            :min="1"
            :max="999"
            size="small"
            fluid
            @update:modelValue="onFieldChange"
          />
        </div>

        <div class="col-action">
          <Button
            icon="pi pi-times"
            text
            rounded
            severity="danger"
            size="small"
            aria-label="Remove slot"
            @click="removeSlot(index)"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.slot-editor {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.slot-editor-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.slot-editor-label {
  font-size: 0.875rem;
  font-weight: 500;
}

.slot-empty {
  font-size: 0.8125rem;
  color: var(--p-text-muted-color);
  padding: 0.75rem;
  border: 1px dashed var(--p-surface-300);
  border-radius: 6px;
  text-align: center;
}

.slot-list {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}

.slot-list-header,
.slot-row {
  display: grid;
  grid-template-columns: 1fr 1fr 100px 36px;
  gap: 0.5rem;
  align-items: start;
}

.slot-list-header {
  font-size: 0.75rem;
  font-weight: 500;
  color: var(--p-text-muted-color);
  padding: 0 0.25rem;
}

.slot-row {
  align-items: center;
}

.col-key {
  display: flex;
  flex-direction: column;
  gap: 0.125rem;
}

.slot-error {
  font-size: 0.7rem;
  color: var(--p-red-500);
}

.col-action {
  display: flex;
  justify-content: center;
}
</style>
