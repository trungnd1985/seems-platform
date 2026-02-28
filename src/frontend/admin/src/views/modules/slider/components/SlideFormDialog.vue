<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Select from 'primevue/select'
import Button from 'primevue/button'
import Message from 'primevue/message'
import type { MediaItem } from '@/types/media'
import MediaPickerDialog from '@/components/MediaPickerDialog.vue'

export interface SlideFormPayload {
  title: string
  subtitle?: string
  imageUrl: string
  ctaText?: string
  ctaLink?: string
  order: number
  status?: string
}

export interface SlideRecord extends SlideFormPayload {
  id: string
  createdAt: string
  updatedAt: string
}

const props = defineProps<{
  visible: boolean
  slide: SlideRecord | null
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  saved: [payload: SlideFormPayload]
}>()

const title = ref('')
const subtitle = ref('')
const imageUrl = ref('')
const ctaText = ref('')
const ctaLink = ref('')
const order = ref(0)
const status = ref('Draft')
const submitting = ref(false)
const serverError = ref<string | null>(null)
const pickerVisible = ref(false)

const isEdit = computed(() => props.slide !== null)

const statusOptions = [
  { label: 'Draft', value: 'Draft' },
  { label: 'Published', value: 'Published' },
]

watch(
  () => props.slide,
  (s) => {
    if (s) {
      title.value = s.title
      subtitle.value = s.subtitle ?? ''
      imageUrl.value = s.imageUrl
      ctaText.value = s.ctaText ?? ''
      ctaLink.value = s.ctaLink ?? ''
      order.value = s.order
      status.value = s.status ?? 'Draft'
    } else {
      title.value = ''
      subtitle.value = ''
      imageUrl.value = ''
      ctaText.value = ''
      ctaLink.value = ''
      order.value = 0
      status.value = 'Draft'
    }
    serverError.value = null
    submitting.value = false
  },
  { immediate: true },
)

const canSubmit = computed(
  () => title.value.trim() && imageUrl.value.trim() && !submitting.value,
)

function onImagePicked(item: MediaItem) {
  imageUrl.value = item.url
}

function close() {
  emit('update:visible', false)
}

async function submit() {
  if (!canSubmit.value) return
  serverError.value = null
  submitting.value = true
  try {
    emit('saved', {
      title: title.value.trim(),
      subtitle: subtitle.value.trim() || undefined,
      imageUrl: imageUrl.value.trim(),
      ctaText: ctaText.value.trim() || undefined,
      ctaLink: ctaLink.value.trim() || undefined,
      order: order.value,
      status: isEdit.value ? status.value : undefined,
    })
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <Dialog
    :visible="visible"
    :header="isEdit ? `Edit: ${slide?.title}` : 'New Slide'"
    :style="{ width: '600px' }"
    :closable="!submitting"
    modal
    @update:visible="close"
  >
    <div class="slide-form">
      <Message v-if="serverError" severity="error">{{ serverError }}</Message>

      <div class="form-row">
        <div class="field field-wide">
          <label for="sl-title">Title <span class="required">*</span></label>
          <InputText
            id="sl-title"
            v-model="title"
            placeholder="e.g. Welcome to SEEMS"
            maxlength="256"
            fluid
            autofocus
          />
        </div>

        <div class="field">
          <label for="sl-order">Order</label>
          <InputNumber
            id="sl-order"
            v-model="order"
            :min="0"
            :max="999"
            fluid
          />
          <small class="hint">Lower numbers appear first.</small>
        </div>
      </div>

      <div class="field">
        <label for="sl-subtitle">Subtitle</label>
        <InputText
          id="sl-subtitle"
          v-model="subtitle"
          placeholder="e.g. Build amazing websites with ease"
          maxlength="512"
          fluid
        />
      </div>

      <!-- Image field: thumbnail preview + URL input + library picker -->
      <div class="field">
        <label>Image URL <span class="required">*</span></label>
        <div class="image-field">
          <div v-if="imageUrl" class="image-thumb">
            <img :src="imageUrl" alt="Slide preview" />
          </div>
          <div class="image-input-row">
            <InputText
              id="sl-image"
              v-model="imageUrl"
              placeholder="https://..."
              maxlength="2048"
              fluid
            />
            <Button
              icon="pi pi-images"
              severity="secondary"
              v-tooltip.top="'Choose from Media Library'"
              :disabled="submitting"
              @click="pickerVisible = true"
            />
          </div>
        </div>
        <small class="hint">Use a high-resolution image (min 1280×500 px recommended).</small>
      </div>

      <div class="form-row">
        <div class="field">
          <label for="sl-cta-text">CTA Button Text</label>
          <InputText
            id="sl-cta-text"
            v-model="ctaText"
            placeholder="e.g. Learn More"
            maxlength="64"
            fluid
          />
        </div>

        <div class="field">
          <label for="sl-cta-link">CTA Button Link</label>
          <InputText
            id="sl-cta-link"
            v-model="ctaLink"
            placeholder="/contact"
            maxlength="512"
            fluid
          />
        </div>
      </div>

      <div v-if="isEdit" class="field">
        <label for="sl-status">Status</label>
        <Select
          id="sl-status"
          v-model="status"
          :options="statusOptions"
          option-label="label"
          option-value="value"
          fluid
        />
      </div>
    </div>

    <template #footer>
      <Button label="Cancel" text :disabled="submitting" @click="close" />
      <Button
        :label="isEdit ? 'Save Changes' : 'Create'"
        :loading="submitting"
        :disabled="!canSubmit"
        @click="submit"
      />
    </template>
  </Dialog>

  <!-- Media library picker — opened from the image field -->
  <MediaPickerDialog
    :visible="pickerVisible"
    @update:visible="pickerVisible = $event"
    @selected="onImagePicked"
  />
</template>

<style scoped>
.slide-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
  padding: 0.25rem 0;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.field-wide {
  grid-column: span 1;
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

/* ── Image field ─────────────────────────────────────────────────────── */
.image-field {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.image-thumb {
  width: 100%;
  height: 120px;
  border-radius: 6px;
  overflow: hidden;
  border: 1px solid var(--p-content-border-color);
  background: var(--p-content-hover-background);
}
.image-thumb img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.image-input-row {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}

.required {
  color: var(--p-red-500);
}

.hint {
  color: var(--p-text-muted-color);
  font-size: 0.75rem;
}
</style>
