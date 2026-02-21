<script setup lang="ts">
import { ref, watch } from 'vue'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Button from 'primevue/button'
import type { Theme } from '@/types/themes'

const props = defineProps<{
  visible: boolean
  theme: Theme | null
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  saved: [payload: { key: string; name: string; description: string | null; cssUrl: string | null }]
}>()

const key = ref('')
const name = ref('')
const description = ref('')
const cssUrl = ref('')

watch(
  () => props.theme,
  (t) => {
    key.value = t?.key ?? ''
    name.value = t?.name ?? ''
    description.value = t?.description ?? ''
    cssUrl.value = t?.cssUrl ?? ''
  },
  { immediate: true },
)

const isEdit = () => props.theme !== null

function close() {
  emit('update:visible', false)
}

function submit() {
  emit('saved', {
    key: key.value.trim(),
    name: name.value.trim(),
    description: description.value.trim() || null,
    cssUrl: cssUrl.value.trim() || null,
  })
}
</script>

<template>
  <Dialog
    :visible="visible"
    :header="isEdit() ? 'Edit Theme' : 'New Theme'"
    :style="{ width: '460px' }"
    modal
    @update:visible="close"
  >
    <form class="theme-form" @submit.prevent="submit">
      <div class="field">
        <label for="theme-key">Key <span class="required">*</span></label>
        <InputText
          id="theme-key"
          v-model="key"
          :disabled="isEdit()"
          placeholder="e.g. minimal-dark"
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
        <label for="theme-name">Name <span class="required">*</span></label>
        <InputText
          id="theme-name"
          v-model="name"
          placeholder="e.g. Minimal Dark"
          maxlength="256"
          fluid
        />
      </div>

      <div class="field">
        <label for="theme-description">Description</label>
        <Textarea
          id="theme-description"
          v-model="description"
          placeholder="Briefly describe this theme..."
          rows="3"
          maxlength="1024"
          fluid
        />
      </div>

      <div class="field">
        <label for="theme-css-url">CSS URL</label>
        <InputText
          id="theme-css-url"
          v-model="cssUrl"
          placeholder="https://cdn.example.com/themes/my-theme/style.css"
          maxlength="2048"
          fluid
        />
        <small class="hint">Public URL to the theme stylesheet. Leave blank if styles are not yet deployed.</small>
      </div>
    </form>

    <template #footer>
      <Button label="Cancel" text @click="close" />
      <Button
        :label="isEdit() ? 'Save Changes' : 'Create Theme'"
        :disabled="!name.trim() || (!isEdit() && !key.trim())"
        @click="submit"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.theme-form {
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

.required {
  color: var(--p-red-500);
}

.hint {
  color: var(--p-text-muted-color);
  font-size: 0.75rem;
}
</style>
