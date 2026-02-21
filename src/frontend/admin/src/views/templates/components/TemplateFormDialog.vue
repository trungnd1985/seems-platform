<script setup lang="ts">
import { ref, watch } from 'vue'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import Button from 'primevue/button'
import SlotEditor from './SlotEditor.vue'
import type { Template, TemplateSlotDef } from '@/types/templates'
import type { Theme } from '@/types/themes'

const props = defineProps<{
  visible: boolean
  template: Template | null
  themes: Theme[]
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  saved: [payload: { key: string; name: string; themeKey: string; slots: TemplateSlotDef[] }]
}>()

const key = ref('')
const name = ref('')
const themeKey = ref('')
const slots = ref<TemplateSlotDef[]>([])

watch(
  () => props.template,
  (t) => {
    key.value = t?.key ?? ''
    name.value = t?.name ?? ''
    themeKey.value = t?.themeKey ?? ''
    slots.value = t ? t.slots.map((s) => ({ ...s })) : []
  },
  { immediate: true },
)

const isEdit = () => props.template !== null

const themeOptions = () =>
  props.themes.map((t) => ({ label: `${t.name} (${t.key})`, value: t.key }))

function close() {
  emit('update:visible', false)
}

function canSubmit(): boolean {
  if (!name.value.trim() || !themeKey.value) return false
  if (!isEdit() && !key.value.trim()) return false
  const hasInvalidSlot = slots.value.some(
    (s) => !s.key.trim() || !s.label.trim() || !/^[a-z][a-z0-9_-]*$/.test(s.key),
  )
  const hasDuplicateKeys =
    new Set(slots.value.map((s) => s.key.trim())).size !== slots.value.length
  return !hasInvalidSlot && !hasDuplicateKeys
}

function submit() {
  emit('saved', {
    key: key.value.trim(),
    name: name.value.trim(),
    themeKey: themeKey.value,
    slots: slots.value.map((s) => ({
      key: s.key.trim(),
      label: s.label.trim(),
      maxItems: s.maxItems ?? undefined,
    })),
  })
}
</script>

<template>
  <Dialog
    :visible="visible"
    :header="isEdit() ? 'Edit Template' : 'New Template'"
    :style="{ width: '580px' }"
    modal
    @update:visible="close"
  >
    <form class="template-form" @submit.prevent="submit">
      <div class="field">
        <label for="tpl-key">Key <span class="required">*</span></label>
        <InputText
          id="tpl-key"
          v-model="key"
          :disabled="isEdit()"
          placeholder="e.g. landing-page"
          maxlength="128"
          fluid
          autofocus
        />
        <small class="hint">
          <template v-if="isEdit()">Key cannot be changed after creation.</template>
          <template v-else>Lowercase letters, digits, hyphens, underscores. Must start with a letter.</template>
        </small>
      </div>

      <div class="field">
        <label for="tpl-name">Name <span class="required">*</span></label>
        <InputText
          id="tpl-name"
          v-model="name"
          placeholder="e.g. Landing Page"
          maxlength="256"
          fluid
        />
      </div>

      <div class="field">
        <label for="tpl-theme">Theme <span class="required">*</span></label>
        <Select
          id="tpl-theme"
          :modelValue="themeKey"
          @update:modelValue="themeKey = $event"
          :options="themeOptions()"
          option-label="label"
          option-value="value"
          placeholder="Select a theme"
          fluid
        />
        <small v-if="themes.length === 0" class="hint warn">
          No themes available. Create a theme first.
        </small>
      </div>

      <div class="field">
        <SlotEditor v-model="slots" />
      </div>
    </form>

    <template #footer>
      <Button label="Cancel" text @click="close" />
      <Button
        :label="isEdit() ? 'Save Changes' : 'Create Template'"
        :disabled="!canSubmit()"
        @click="submit"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.template-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
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

.hint.warn {
  color: var(--p-orange-500);
}
</style>
