<script setup lang="ts">
import { ref, watch } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import { useToast } from 'primevue/usetoast'
import { usePages } from '@/composables/usePages'

const props = defineProps<{
  visible: boolean
  pageId: string
  slotId: string
  slotKey: string
  initialParameters?: Record<string, unknown> | null
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  saved: [parameters: Record<string, unknown> | null]
}>()

interface ParamRow {
  key: string
  value: string
}

const toast = useToast()
const { updateSlotParameters } = usePages()
const saving = ref(false)
const rows = ref<ParamRow[]>([])

watch(
  () => props.visible,
  (open) => {
    if (!open) return
    const existing = props.initialParameters ?? {}
    rows.value = Object.entries(existing).map(([k, v]) => ({
      key: k,
      value: typeof v === 'string' ? v : JSON.stringify(v),
    }))
  },
  { immediate: true },
)

function addRow() {
  rows.value.push({ key: '', value: '' })
}

function removeRow(index: number) {
  rows.value.splice(index, 1)
}

function parseValue(raw: string): unknown {
  const trimmed = raw.trim()
  if (trimmed === 'true') return true
  if (trimmed === 'false') return false
  if (trimmed !== '' && !isNaN(Number(trimmed))) return Number(trimmed)
  try {
    return JSON.parse(trimmed)
  } catch {
    return raw
  }
}

async function save() {
  const deduped: Record<string, unknown> = {}
  for (const row of rows.value) {
    const k = row.key.trim()
    if (!k) continue
    deduped[k] = parseValue(row.value)
  }
  const params = Object.keys(deduped).length > 0 ? deduped : null
  saving.value = true
  try {
    await updateSlotParameters(props.pageId, props.slotId, params)
    emit('saved', params)
    emit('update:visible', false)
    toast.add({ severity: 'success', summary: 'Parameters saved', life: 2000 })
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: 'Failed to save parameters',
      detail: e.response?.data?.message ?? 'Error.',
      life: 4000,
    })
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <Dialog
    :visible="visible"
    @update:visible="$emit('update:visible', $event)"
    modal
    :header="`Slot parameters — ${slotKey}`"
    style="width: 560px"
  >
    <div class="params-editor">
      <div v-if="rows.length === 0" class="empty-hint">
        No parameters configured. Click <strong>Add row</strong> to begin.
      </div>

      <div v-for="(row, idx) in rows" :key="idx" class="param-row">
        <InputText
          v-model="row.key"
          placeholder="key"
          class="param-key"
        />
        <span class="param-sep">=</span>
        <InputText
          v-model="row.value"
          placeholder="value (string, number, true/false, JSON)"
          class="param-value"
        />
        <Button
          icon="pi pi-times"
          text
          rounded
          severity="danger"
          size="small"
          aria-label="Remove row"
          @click="removeRow(idx)"
        />
      </div>

      <Button
        icon="pi pi-plus"
        label="Add row"
        text
        severity="secondary"
        size="small"
        class="add-row-btn"
        @click="addRow"
      />
    </div>

    <template #footer>
      <Button
        label="Cancel"
        severity="secondary"
        text
        @click="$emit('update:visible', false)"
      />
      <Button
        label="Save"
        icon="pi pi-check"
        :loading="saving"
        @click="save"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.params-editor {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  min-height: 80px;
}

.empty-hint {
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
  padding: 0.5rem 0;
}

.param-row {
  display: flex;
  align-items: center;
  gap: 0.4rem;
}

.param-key {
  width: 160px;
  flex-shrink: 0;
  font-family: monospace;
  font-size: 0.875rem;
}

.param-sep {
  color: var(--p-text-muted-color);
  font-weight: 600;
}

.param-value {
  flex: 1;
  font-family: monospace;
  font-size: 0.875rem;
}

.add-row-btn {
  align-self: flex-start;
  margin-top: 0.25rem;
}
</style>
