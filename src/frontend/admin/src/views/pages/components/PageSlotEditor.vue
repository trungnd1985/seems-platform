<script setup lang="ts">
import { ref, computed } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Select from 'primevue/select'
import InputText from 'primevue/inputtext'
import Tag from 'primevue/tag'
import Toast from 'primevue/toast'
import { useToast } from 'primevue/usetoast'
import { usePages } from '@/composables/usePages'
import type { Page, SlotMapping, SlotTargetType } from '@/types/pages'
import { SLOT_TARGET_TYPES } from '@/types/pages'
import type { TemplateSlotDef } from '@/types/templates'

const props = defineProps<{
  page: Page
  templateSlots: TemplateSlotDef[]
  initialSlots: SlotMapping[]
}>()

const emit = defineEmits<{
  updated: [slots: SlotMapping[]]
}>()

const toast = useToast()
const { addSlot, removeSlot, reorderSlots } = usePages()

// Local sorted copy — mutated directly; never triggers parent re-render unless we emit
const slots = ref<SlotMapping[]>([...props.initialSlots].sort((a, b) => a.order - b.order))

// ── Add slot form ─────────────────────────────────────────────────────────────
const newSlotKey = ref('')
const newTargetType = ref<SlotTargetType>('Content')
const newTargetId = ref('')
const adding = ref(false)

const slotKeyOptions = computed(() => {
  if (props.templateSlots.length > 0) {
    return props.templateSlots.map((s) => ({ label: `${s.label} (${s.key})`, value: s.key }))
  }
  return []
})

const canAdd = computed(() => !!newSlotKey.value && !!newTargetId.value.trim())

async function doAdd() {
  if (!canAdd.value) return
  adding.value = true
  try {
    const mapping = await addSlot({
      pageId: props.page.id,
      slotKey: newSlotKey.value,
      targetType: newTargetType.value,
      targetId: newTargetId.value.trim(),
    })
    slots.value = [...slots.value, mapping].sort((a, b) => a.order - b.order)
    newTargetId.value = ''
    emit('updated', slots.value)
    toast.add({ severity: 'success', summary: 'Slot added', life: 2000 })
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: 'Failed to add slot',
      detail: e.response?.data?.message ?? 'Error.',
      life: 4000,
    })
  } finally {
    adding.value = false
  }
}

// ── Remove slot ───────────────────────────────────────────────────────────────
const removingId = ref<string | null>(null)

async function doRemove(slotId: string) {
  removingId.value = slotId
  try {
    await removeSlot(props.page.id, slotId)
    slots.value = slots.value.filter((s) => s.id !== slotId)
    emit('updated', slots.value)
    toast.add({ severity: 'success', summary: 'Slot removed', life: 2000 })
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: 'Failed to remove slot',
      detail: e.response?.data?.message ?? 'Error.',
      life: 4000,
    })
  } finally {
    removingId.value = null
  }
}

// ── Reorder within the same slotKey ──────────────────────────────────────────
async function move(slotId: string, direction: 'up' | 'down') {
  const item = slots.value.find((s) => s.id === slotId)
  if (!item) return

  // Work only with slots sharing the same slotKey, sorted by current order
  const group = [...slots.value]
    .filter((s) => s.slotKey === item.slotKey)
    .sort((a, b) => a.order - b.order)

  const idx = group.findIndex((s) => s.id === slotId)
  const targetIdx = direction === 'up' ? idx - 1 : idx + 1
  if (targetIdx < 0 || targetIdx >= group.length) return

  // Swap and reassign sequential orders
  ;[group[idx], group[targetIdx]] = [group[targetIdx], group[idx]]
  const reindexed = group.map((s, i) => ({ ...s, order: i }))

  // Merge reindexed group back into full list
  const updated = slots.value.map((s) => reindexed.find((r) => r.id === s.id) ?? s)

  try {
    await reorderSlots(
      props.page.id,
      updated.map((s) => ({ slotId: s.id, order: s.order })),
    )
    slots.value = [...updated].sort((a, b) => {
      const keyDiff = a.slotKey.localeCompare(b.slotKey)
      return keyDiff !== 0 ? keyDiff : a.order - b.order
    })
    emit('updated', slots.value)
  } catch (e: any) {
    toast.add({
      severity: 'error',
      summary: 'Failed to reorder',
      detail: e.response?.data?.message ?? 'Error.',
      life: 4000,
    })
  }
}

function isFirst(slot: SlotMapping): boolean {
  const group = slots.value
    .filter((s) => s.slotKey === slot.slotKey)
    .sort((a, b) => a.order - b.order)
  return group[0]?.id === slot.id
}

function isLast(slot: SlotMapping): boolean {
  const group = slots.value
    .filter((s) => s.slotKey === slot.slotKey)
    .sort((a, b) => a.order - b.order)
  return group[group.length - 1]?.id === slot.id
}
</script>

<template>
  <div class="slot-editor">
    <Toast />

    <!-- Current slots -->
    <DataTable
      :value="slots"
      size="small"
      striped-rows
      :empty-message="'No slots assigned yet.'"
    >
      <Column field="slotKey" header="Slot" style="width: 140px">
        <template #body="{ data }">
          <code class="key-badge">{{ data.slotKey }}</code>
        </template>
      </Column>

      <Column field="targetType" header="Type" style="width: 110px">
        <template #body="{ data }">
          <Tag
            :value="data.targetType"
            :severity="data.targetType === 'Content' ? 'info' : 'warn'"
          />
        </template>
      </Column>

      <Column field="targetId" header="Target ID">
        <template #body="{ data }">
          <code class="target-id">{{ data.targetId }}</code>
        </template>
      </Column>

      <Column header="Order" style="width: 90px; text-align: center">
        <template #body="{ data }">
          <div class="order-buttons">
            <Button
              icon="pi pi-chevron-up"
              text
              rounded
              size="small"
              severity="secondary"
              :disabled="isFirst(data)"
              aria-label="Move up"
              @click="move(data.id, 'up')"
            />
            <Button
              icon="pi pi-chevron-down"
              text
              rounded
              size="small"
              severity="secondary"
              :disabled="isLast(data)"
              aria-label="Move down"
              @click="move(data.id, 'down')"
            />
          </div>
        </template>
      </Column>

      <Column header="" style="width: 50px">
        <template #body="{ data }">
          <Button
            icon="pi pi-times"
            text
            rounded
            size="small"
            severity="danger"
            :loading="removingId === data.id"
            aria-label="Remove"
            @click="doRemove(data.id)"
          />
        </template>
      </Column>
    </DataTable>

    <!-- Add slot form -->
    <div class="add-slot-form">
      <span class="add-slot-label">Add slot</span>

      <div class="add-slot-fields">
        <Select
          v-if="slotKeyOptions.length > 0"
          :modelValue="newSlotKey"
          @update:modelValue="newSlotKey = $event"
          :options="slotKeyOptions"
          option-label="label"
          option-value="value"
          placeholder="Slot key"
          style="min-width: 160px"
        />
        <InputText
          v-else
          v-model="newSlotKey"
          placeholder="slot-key"
          maxlength="64"
          style="width: 140px"
        />

        <Select
          :modelValue="newTargetType"
          @update:modelValue="newTargetType = $event"
          :options="SLOT_TARGET_TYPES"
          option-label="label"
          option-value="value"
          style="width: 140px"
        />

        <InputText
          v-model="newTargetId"
          placeholder="Target ID (UUID)"
          maxlength="256"
          style="flex: 1; min-width: 180px"
          @keydown.enter.prevent="doAdd"
        />

        <Button
          icon="pi pi-plus"
          label="Add"
          :disabled="!canAdd"
          :loading="adding"
          @click="doAdd"
        />
      </div>

      <small v-if="templateSlots.length === 0" class="hint warn">
        No template selected or template has no defined slots — enter a slot key manually.
      </small>
    </div>
  </div>
</template>

<style scoped>
.slot-editor {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding: 0.75rem 0 0.25rem;
}

.key-badge {
  font-family: monospace;
  font-size: 0.8125rem;
  background: var(--p-surface-100);
  border: 1px solid var(--p-surface-200);
  border-radius: 4px;
  padding: 0.125rem 0.375rem;
}

.target-id {
  font-family: monospace;
  font-size: 0.75rem;
  color: var(--p-text-muted-color);
  word-break: break-all;
}

.order-buttons {
  display: flex;
  gap: 0;
}

.add-slot-form {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding: 0.75rem;
  background: var(--p-surface-50);
  border: 1px solid var(--p-surface-200);
  border-radius: 6px;
}

.add-slot-label {
  font-size: 0.8125rem;
  font-weight: 600;
  color: var(--p-text-muted-color);
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.add-slot-fields {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.hint {
  font-size: 0.75rem;
  color: var(--p-text-muted-color);
}

.hint.warn {
  color: var(--p-orange-500);
}
</style>
