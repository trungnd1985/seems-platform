<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Button from 'primevue/button'
import Message from 'primevue/message'
import type { Module, RegisterModuleRequest, UpdateModuleRequest, ContentTypeDecl } from '@/types/modules'

const props = defineProps<{
  visible: boolean
  module?: Module | null
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  saved: [payload: RegisterModuleRequest | UpdateModuleRequest]
}>()

const isEdit = computed(() => !!props.module)

const moduleKey = ref('')
const name = ref('')
const version = ref('')
const publicComponentUrl = ref('')
const description = ref('')
const author = ref('')
const contentTypes = ref<ContentTypeDecl[]>([])
const manifestJson = ref('')
const manifestError = ref<string | null>(null)
const submitting = ref(false)
const serverError = ref<string | null>(null)

watch(
  [() => props.visible, () => props.module],
  ([open]) => {
    if (!open) return
    serverError.value = null
    manifestError.value = null
    manifestJson.value = ''
    submitting.value = false

    if (props.module) {
      moduleKey.value = props.module.moduleKey
      name.value = props.module.name
      version.value = props.module.version
      publicComponentUrl.value = props.module.publicComponentUrl ?? ''
      description.value = props.module.description ?? ''
      author.value = props.module.author ?? ''
      contentTypes.value = []
    } else {
      moduleKey.value = ''
      name.value = ''
      version.value = ''
      publicComponentUrl.value = ''
      description.value = ''
      author.value = ''
      contentTypes.value = []
    }
  },
)

const keyError = computed(() => {
  if (!moduleKey.value) return null
  if (!/^[a-z][a-z0-9-]*$/.test(moduleKey.value))
    return 'Lowercase letters, digits, and hyphens only. Must start with a letter.'
  return null
})

const urlError = computed(() => {
  if (!publicComponentUrl.value) return null
  const url = publicComponentUrl.value
  if (!url.startsWith('/') && !url.startsWith('https://'))
    return 'URL must start with "/" (relative) or "https://".'
  return null
})

const canSubmit = computed(
  () =>
    moduleKey.value.trim() &&
    !keyError.value &&
    name.value.trim() &&
    version.value.trim() &&
    !urlError.value &&
    !submitting.value,
)

function parseManifest() {
  manifestError.value = null
  if (!manifestJson.value.trim()) return
  try {
    const m = JSON.parse(manifestJson.value)
    if (m.moduleKey) moduleKey.value = m.moduleKey
    if (m.name) name.value = m.name
    if (m.version) version.value = m.version
    if (m.description) description.value = m.description
    if (m.author) author.value = m.author
    if (m.publicComponentUrl) publicComponentUrl.value = m.publicComponentUrl
    if (Array.isArray(m.contentTypes)) {
      contentTypes.value = m.contentTypes.map((ct: any) => ({
        key: String(ct.key ?? ''),
        name: String(ct.name ?? ''),
        schema: typeof ct.schema === 'string' ? ct.schema : JSON.stringify(ct.schema),
      }))
    }
    manifestJson.value = ''
  } catch {
    manifestError.value = 'Invalid JSON — paste the full contents of manifest.json.'
  }
}

function removeContentType(index: number) {
  contentTypes.value.splice(index, 1)
}

function close() {
  emit('update:visible', false)
}

async function submit() {
  if (!canSubmit.value) return
  serverError.value = null
  submitting.value = true
  try {
    if (isEdit.value) {
      const payload: UpdateModuleRequest = {
        name: name.value.trim(),
        version: version.value.trim(),
        publicComponentUrl: publicComponentUrl.value.trim() || undefined,
        description: description.value.trim() || undefined,
        author: author.value.trim() || undefined,
      }
      emit('saved', payload)
    } else {
      const payload: RegisterModuleRequest = {
        moduleKey: moduleKey.value.trim(),
        name: name.value.trim(),
        version: version.value.trim(),
        publicComponentUrl: publicComponentUrl.value.trim() || undefined,
        description: description.value.trim() || undefined,
        author: author.value.trim() || undefined,
        contentTypes: contentTypes.value.length ? contentTypes.value : undefined,
      }
      emit('saved', payload)
    }
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <Dialog
    :visible="visible"
    :header="isEdit ? 'Edit: ' + (module?.name ?? '') : 'Register Module'"
    :style="{ width: '640px' }"
    :closable="!submitting"
    modal
    @update:visible="close"
  >
    <div class="module-form">
      <Message v-if="serverError" severity="error">{{ serverError }}</Message>

      <!-- Manifest import (create mode only) -->
      <template v-if="!isEdit">
        <div class="manifest-section">
          <label class="manifest-label">Import from manifest.json</label>
          <div class="manifest-row">
            <Textarea
              v-model="manifestJson"
              placeholder="Paste the contents of manifest.json here…"
              rows="3"
              class="manifest-textarea"
              :disabled="submitting"
            />
            <Button
              label="Parse"
              severity="secondary"
              :disabled="!manifestJson.trim() || submitting"
              @click="parseManifest"
            />
          </div>
          <small v-if="manifestError" class="error-hint">{{ manifestError }}</small>
          <small v-else class="hint">
            Auto-fills the fields below and registers any declared content types.
          </small>
        </div>

        <div class="divider" />
      </template>

      <div class="form-row">
        <div class="field">
          <label for="mod-key">Module Key <span class="required">*</span></label>
          <InputText
            id="mod-key"
            v-model="moduleKey"
            placeholder="e.g. contact-form"
            maxlength="128"
            fluid
            :disabled="isEdit"
            :autofocus="!isEdit"
          />
          <small v-if="keyError && moduleKey" class="error-hint">{{ keyError }}</small>
          <small v-else-if="isEdit" class="hint">Module key is immutable after registration.</small>
          <small v-else class="hint">Unique kebab-case ID. Immutable after registration.</small>
        </div>

        <div class="field">
          <label for="mod-name">Name <span class="required">*</span></label>
          <InputText
            id="mod-name"
            v-model="name"
            placeholder="e.g. Contact Form"
            maxlength="256"
            fluid
            :autofocus="isEdit"
          />
        </div>
      </div>

      <div class="form-row">
        <div class="field">
          <label for="mod-version">Version <span class="required">*</span></label>
          <InputText
            id="mod-version"
            v-model="version"
            placeholder="e.g. 1.0.0"
            maxlength="64"
            fluid
          />
          <small class="hint">Semantic version (e.g. 1.0.0).</small>
        </div>

        <div class="field">
          <label for="mod-author">Author</label>
          <InputText
            id="mod-author"
            v-model="author"
            placeholder="e.g. ACME Corp"
            maxlength="256"
            fluid
          />
        </div>
      </div>

      <div class="field">
        <label for="mod-url">Public Component URL</label>
        <InputText
          id="mod-url"
          v-model="publicComponentUrl"
          placeholder="/modules/my-module/component.js or https://cdn.example.com/…"
          maxlength="2048"
          fluid
        />
        <small v-if="urlError && publicComponentUrl" class="error-hint">{{ urlError }}</small>
        <small v-else class="hint">
          Relative path or HTTPS URL to the pre-compiled ESM bundle. Required for slot rendering.
        </small>
      </div>

      <div class="field">
        <label for="mod-desc">Description</label>
        <InputText
          id="mod-desc"
          v-model="description"
          placeholder="Short description shown in the module list"
          maxlength="512"
          fluid
        />
      </div>

      <!-- Content types summary (create mode only) -->
      <div v-if="!isEdit && contentTypes.length" class="content-types-section">
        <div class="ct-header">
          <span class="ct-label">Content Types</span>
          <span class="ct-count">{{ contentTypes.length }}</span>
        </div>
        <ul class="ct-list">
          <li v-for="(ct, i) in contentTypes" :key="ct.key" class="ct-item">
            <div class="ct-item-info">
              <code class="ct-key">{{ ct.key }}</code>
              <span class="ct-name">{{ ct.name }}</span>
            </div>
            <Button
              icon="pi pi-times"
              text
              rounded
              severity="secondary"
              size="small"
              :aria-label="'Remove ' + ct.key"
              @click="removeContentType(i)"
            />
          </li>
        </ul>
      </div>
    </div>

    <template #footer>
      <Button label="Cancel" text :disabled="submitting" @click="close" />
      <Button
        :label="isEdit ? 'Save Changes' : 'Register'"
        :loading="submitting"
        :disabled="!canSubmit"
        @click="submit"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.module-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
  padding: 0.25rem 0;
}

.manifest-section {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}

.manifest-label {
  font-size: 0.875rem;
  font-weight: 500;
}

.manifest-row {
  display: flex;
  gap: 0.5rem;
  align-items: flex-start;
}

.manifest-textarea {
  flex: 1;
  font-family: monospace;
  font-size: 0.8rem;
  resize: vertical;
}

.divider {
  height: 1px;
  background: var(--p-surface-200);
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
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

.error-hint {
  color: var(--p-red-500);
  font-size: 0.75rem;
}

.content-types-section {
  border: 1px solid var(--p-surface-200);
  border-radius: 6px;
  padding: 0.75rem;
}

.ct-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}

.ct-label {
  font-size: 0.875rem;
  font-weight: 500;
}

.ct-count {
  background: var(--p-primary-color);
  color: #fff;
  font-size: 0.7rem;
  font-weight: 600;
  border-radius: 999px;
  padding: 0.1rem 0.5rem;
  line-height: 1.4;
}

.ct-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.ct-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.25rem 0.25rem 0.25rem 0;
}

.ct-item-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.ct-key {
  font-size: 0.8rem;
  font-family: monospace;
  background: var(--p-surface-100);
  padding: 0.1rem 0.35rem;
  border-radius: 3px;
  color: var(--p-text-muted-color);
}

.ct-name {
  font-size: 0.875rem;
}
</style>
