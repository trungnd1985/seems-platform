<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Select from 'primevue/select'
import InputText from 'primevue/inputtext'
import AutoComplete from 'primevue/autocomplete'
import Tag from 'primevue/tag'
import Toast from 'primevue/toast'
import { useToast } from 'primevue/usetoast'
import { usePages } from '@/composables/usePages'
import { useContentItems } from '@/composables/useContentItems'
import { useModules } from '@/composables/useModules'
import type { Page, SlotMapping, SlotTargetType } from '@/types/pages'
import { SLOT_TARGET_TYPES } from '@/types/pages'
import type { TemplateSlotDef } from '@/types/templates'
import type { ContentItem } from '@/types/contentTypes'
import type { Module } from '@/types/modules'

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
const { items: contentItemResults, loading: contentItemLoading, fetchContentItems } = useContentItems()
const { modules, fetchModules } = useModules()

onMounted(() => { void fetchModules() })

// Local sorted copy — mutated directly; never triggers parent re-render unless we emit
const slots = ref<SlotMapping[]>([...props.initialSlots].sort((a, b) => a.order - b.order))

// ── Add slot form ─────────────────────────────────────────────────────────────
const newSlotKey = ref('')
const newTargetType = ref<SlotTargetType>('Content')
const newTargetId = ref('')
const adding = ref(false)

// Content item picker state (used when targetType === 'Content')
const selectedContentItem = ref<ContentItem | null>(null)

// Module picker state (used when targetType === 'Module')
const selectedModule = ref<Module | null>(null)
const moduleResults = ref<Module[]>([])

// Clear selection when target type changes
watch(newTargetType, () => {
  newTargetId.value = ''
  selectedContentItem.value = null
  selectedModule.value = null
  moduleResults.value = []
})

// Sync selected content item ID → newTargetId
watch(selectedContentItem, (item) => {
  newTargetId.value = (item && typeof item === 'object') ? item.id : ''
})

// Sync selected module key → newTargetId
watch(selectedModule, (m) => {
  newTargetId.value = (m && typeof m === 'object') ? m.moduleKey : ''
})

async function onContentItemSearch(event: { query: string }) {
  await fetchContentItems({ search: event.query || undefined, pageSize: 20 })
}

function contentItemLabel(item: ContentItem): string {
  const data = item.data as Record<string, unknown>
  const title = (data?.title ?? data?.name ?? null) as string | null
  return title
    ? `${title} · ${item.contentTypeKey}`
    : `${item.contentTypeKey} · ${item.id.slice(0, 8)}…`
}

function onModuleSearch(event: { query: string }) {
  const q = event.query.toLowerCase()
  moduleResults.value = modules.value.filter(
    (m) =>
      m.status === 'Installed' &&
      (m.name.toLowerCase().includes(q) || m.moduleKey.toLowerCase().includes(q)),
  )
}

function moduleLabel(m: Module): string {
  return `${m.name} (${m.moduleKey})`
}

const slotKeyOptions = computed(() => {
  if (props.templateSlots.length > 0) {
    return props.templateSlots.map((s) => ({ label: `${s.label} (${s.key})`, value: s.key }))
  }
  return []
})

const canAdd = computed(() => {
  if (!newSlotKey.value) return false
  if (newTargetType.value === 'Content') {
    return !!selectedContentItem.value && typeof selectedContentItem.value === 'object'
  }
  return !!newTargetId.value.trim()
})

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
    selectedContentItem.value = null
    selectedModule.value = null
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

      <Column field="targetId" header="Target">
        <template #body="{ data }">
          <template v-if="data.targetType === 'Module'">
            <span class="target-module">
              <span class="target-module__name">
                {{ modules.find((m) => m.moduleKey === data.targetId)?.name ?? data.targetId }}
              </span>
              <code class="target-id">{{ data.targetId }}</code>
            </span>
          </template>
          <code v-else class="target-id">{{ data.targetId }}</code>
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

        <!-- Content item: searchable picker -->
        <AutoComplete
          v-if="newTargetType === 'Content'"
          v-model="selectedContentItem"
          :suggestions="contentItemResults"
          :loading="contentItemLoading"
          :option-label="contentItemLabel"
          force-selection
          placeholder="Search content items…"
          style="flex: 1; min-width: 220px"
          @complete="onContentItemSearch"
        >
          <template #option="{ option }">
            <div class="ci-option">
              <span class="ci-label">{{ contentItemLabel(option) }}</span>
              <Tag
                :value="option.status"
                :severity="option.status === 'Published' ? 'success' : 'secondary'"
                class="ci-status"
              />
            </div>
          </template>
        </AutoComplete>

        <!-- Module: searchable picker (Installed modules only) -->
        <AutoComplete
          v-else
          v-model="selectedModule"
          :suggestions="moduleResults"
          :option-label="moduleLabel"
          force-selection
          placeholder="Search modules…"
          style="flex: 1; min-width: 220px"
          @complete="onModuleSearch"
        >
          <template #option="{ option }">
            <div class="module-option">
              <div class="module-option__main">
                <span class="module-option__name">{{ option.name }}</span>
                <code class="module-option__key">{{ option.moduleKey }}</code>
              </div>
              <Tag
                v-if="option.description"
                :value="option.version"
                severity="secondary"
                class="module-option__ver"
              />
            </div>
          </template>
          <template #empty>No installed modules match your search.</template>
        </AutoComplete>

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

.target-module {
  display: flex;
  align-items: center;
  gap: 0.4rem;
}

.target-module__name {
  font-size: 0.875rem;
  font-weight: 500;
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

.ci-option {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  width: 100%;
}

.ci-label {
  font-size: 0.875rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.ci-status {
  flex-shrink: 0;
  font-size: 0.7rem;
}

.hint {
  font-size: 0.75rem;
  color: var(--p-text-muted-color);
}

.hint.warn {
  color: var(--p-orange-500);
}

.module-option {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  width: 100%;
}

.module-option__main {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  min-width: 0;
}

.module-option__name {
  font-size: 0.875rem;
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.module-option__key {
  font-family: monospace;
  font-size: 0.75rem;
  color: var(--p-text-muted-color);
  background: var(--p-surface-100);
  border-radius: 3px;
  padding: 0.05rem 0.3rem;
  flex-shrink: 0;
}

.module-option__ver {
  flex-shrink: 0;
  font-size: 0.7rem;
}
</style>
