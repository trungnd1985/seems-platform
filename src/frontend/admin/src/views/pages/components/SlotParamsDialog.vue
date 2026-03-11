<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Message from 'primevue/message'
import { useToast } from 'primevue/usetoast'
import { usePages } from '@/composables/usePages'

const props = defineProps<{
  visible: boolean
  pageId: string
  slotId: string
  slotKey: string
  targetType?: string | null
  moduleDefaultParametersJson?: string | null
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

// ── Mode ───────────────────────────────────────────────────────────────────
// Module slots use a JSON textarea; Content slots use key=value rows.
const isModuleSlot = computed(() => props.targetType === 'Module')

// ── JSON editor (module mode) ──────────────────────────────────────────────
const jsonText = ref('')
const jsonError = ref<string | null>(null)

function validateJson(text: string): boolean {
  jsonError.value = null
  if (!text.trim()) return true
  try {
    JSON.parse(text)
    return true
  } catch (e: any) {
    jsonError.value = `Invalid JSON: ${e.message}`
    return false
  }
}

function loadModuleDefaults() {
  if (!props.moduleDefaultParametersJson) return
  jsonText.value = JSON.stringify(JSON.parse(props.moduleDefaultParametersJson), null, 2)
  jsonError.value = null
}

// ── Key=value rows (content mode) ─────────────────────────────────────────
const rows = ref<ParamRow[]>([])

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

// ── Sync on open ──────────────────────────────────────────────────────────
watch(
  () => props.visible,
  (open) => {
    if (!open) return
    jsonError.value = null

    if (isModuleSlot.value) {
      // Populate JSON textarea from existing parameters, or load module defaults
      if (props.initialParameters && Object.keys(props.initialParameters).length > 0) {
        jsonText.value = JSON.stringify(props.initialParameters, null, 2)
      } else if (props.moduleDefaultParametersJson) {
        loadModuleDefaults()
      } else {
        jsonText.value = '{}'
      }
    } else {
      const existing = props.initialParameters ?? {}
      rows.value = Object.entries(existing).map(([k, v]) => ({
        key: k,
        value: typeof v === 'string' ? v : JSON.stringify(v),
      }))
    }
  },
  { immediate: true },
)

// ── Save ──────────────────────────────────────────────────────────────────
async function save() {
  let params: Record<string, unknown> | null

  if (isModuleSlot.value) {
    const trimmed = jsonText.value.trim()
    if (trimmed && !validateJson(trimmed)) return
    if (!trimmed || trimmed === '{}') {
      params = null
    } else {
      params = JSON.parse(trimmed) as Record<string, unknown>
    }
  } else {
    const deduped: Record<string, unknown> = {}
    for (const row of rows.value) {
      const k = row.key.trim()
      if (!k) continue
      deduped[k] = parseValue(row.value)
    }
    params = Object.keys(deduped).length > 0 ? deduped : null
  }

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
    style="width: 620px"
  >
    <!-- ── Module slot: JSON editor ── -->
    <div v-if="isModuleSlot" class="json-editor">
      <div class="json-toolbar">
        <span class="json-hint">Edit parameters as JSON.</span>
        <Button
          v-if="moduleDefaultParametersJson"
          label="Load defaults"
          icon="pi pi-refresh"
          text
          severity="secondary"
          size="small"
          @click="loadModuleDefaults"
        />
      </div>
      <Textarea
        v-model="jsonText"
        class="json-textarea"
        :class="{ 'json-textarea--error': jsonError }"
        rows="14"
        spellcheck="false"
        autocomplete="off"
        @input="jsonError = null"
      />
      <Message v-if="jsonError" severity="error" class="json-error">{{ jsonError }}</Message>
    </div>

    <!-- ── Content slot: key=value rows ── -->
    <div v-else class="params-editor">
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
/* ── JSON editor ── */
.json-editor {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.json-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.json-hint {
  font-size: 0.8125rem;
  color: var(--p-text-muted-color);
}

.json-textarea {
  width: 100%;
  font-family: monospace;
  font-size: 0.8rem;
  line-height: 1.5;
  resize: vertical;
  border-radius: 6px;
}

.json-textarea--error {
  border-color: var(--p-red-400);
}

.json-error {
  margin: 0;
}

/* ── Key=value editor ── */
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
