<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import Dialog from 'primevue/dialog'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import InputNumber from 'primevue/inputnumber'
import Select from 'primevue/select'
import type { Category } from '@/types/categoryTypes'

const props = defineProps<{
  visible: boolean
  category: Category | null
  contentTypeKey: string
  allCategories: Category[]
  defaultParentId?: string | null
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  saved: [payload: { name: string; slug: string; description: string; parentId: string | null; sortOrder: number }]
}>()

const name = ref('')
const slug = ref('')
const description = ref('')
const parentId = ref<string | null>(null)
const sortOrder = ref(0)
const slugManuallyEdited = ref(false)
const submitting = ref(false)
const nameError = ref('')

const isEdit = computed(() => props.category !== null)

watch(
  () => [props.visible, props.category] as const,
  ([visible, cat]) => {
    if (!visible) return
    nameError.value = ''
    submitting.value = false
    slugManuallyEdited.value = false

    if (cat) {
      name.value = cat.name
      slug.value = cat.slug
      description.value = cat.description ?? ''
      parentId.value = cat.parentId
      sortOrder.value = cat.sortOrder
    } else {
      name.value = ''
      slug.value = ''
      description.value = ''
      parentId.value = props.defaultParentId ?? null
      sortOrder.value = 0
    }
  },
  { immediate: true },
)

watch(name, (val) => {
  if (!slugManuallyEdited.value) {
    slug.value = buildSlug(val)
  }
})

function buildSlug(s: string): string {
  return s
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9\s-]/g, '')
    .replace(/[\s-]+/g, '-')
    .replace(/^-+|-+$/g, '')
}

function onSlugInput(val: string) {
  slugManuallyEdited.value = true
  slug.value = val
}

// Flatten tree into options, excluding self and descendants to prevent circular parent
function flatParentOptions(): { label: string; value: string }[] {
  const result: { label: string; value: string }[] = []
  const selfId = props.category?.id

  function getDescendantIds(id: string): Set<string> {
    const ids = new Set<string>([id])
    const children = props.allCategories.filter((c) => c.parentId === id)
    for (const child of children) {
      getDescendantIds(child.id).forEach((d) => ids.add(d))
    }
    return ids
  }

  const excluded = selfId ? getDescendantIds(selfId) : new Set<string>()

  function walk(categories: Category[], depth: number) {
    for (const c of categories) {
      if (excluded.has(c.id)) continue
      result.push({ label: '  '.repeat(depth) + c.name, value: c.id })
      walk(props.allCategories.filter((ch) => ch.parentId === c.id), depth + 1)
    }
  }

  walk(props.allCategories.filter((c) => c.parentId === null), 0)
  return result
}

const parentOptions = computed(() => flatParentOptions())

function close() {
  emit('update:visible', false)
}

function submit() {
  nameError.value = ''
  if (!name.value.trim()) {
    nameError.value = 'Name is required.'
    return
  }
  submitting.value = true
  emit('saved', {
    name: name.value.trim(),
    slug: slug.value || buildSlug(name.value),
    description: description.value.trim(),
    parentId: parentId.value,
    sortOrder: sortOrder.value,
  })
  submitting.value = false
}
</script>

<template>
  <Dialog
    :visible="visible"
    :header="isEdit ? 'Edit Category' : 'New Category'"
    :style="{ width: '520px', maxWidth: '96vw' }"
    :closable="!submitting"
    modal
    @update:visible="close"
  >
    <div class="cat-form">
      <div class="field">
        <label for="cat-name">Name <span class="required">*</span></label>
        <InputText
          id="cat-name"
          :model-value="name"
          @update:model-value="name = $event as string"
          :invalid="!!nameError"
          fluid
          autofocus
        />
        <small v-if="nameError" class="error-hint">{{ nameError }}</small>
      </div>

      <div class="field">
        <label for="cat-slug">Slug</label>
        <InputText
          id="cat-slug"
          :model-value="slug"
          @update:model-value="onSlugInput($event as string)"
          placeholder="auto-generated-from-name"
          fluid
        />
        <small class="hint">Used in URLs — lowercase, hyphens only.</small>
      </div>

      <div class="field">
        <label for="cat-desc">Description</label>
        <Textarea
          id="cat-desc"
          :model-value="description"
          @update:model-value="description = $event as string"
          rows="3"
          fluid
          auto-resize
        />
      </div>

      <div class="row-2col">
        <div class="field">
          <label for="cat-parent">Parent Category</label>
          <Select
            id="cat-parent"
            :model-value="parentId"
            @update:model-value="parentId = $event as string | null"
            :options="parentOptions"
            option-label="label"
            option-value="value"
            placeholder="None (root)"
            show-clear
            fluid
          />
        </div>

        <div class="field">
          <label for="cat-order">Sort Order</label>
          <InputNumber
            id="cat-order"
            :model-value="sortOrder"
            @update:model-value="sortOrder = $event ?? 0"
            :min="0"
            fluid
          />
        </div>
      </div>
    </div>

    <template #footer>
      <Button label="Cancel" text :disabled="submitting" @click="close" />
      <Button
        :label="isEdit ? 'Save Changes' : 'Create'"
        :loading="submitting"
        @click="submit"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.cat-form {
  display: flex;
  flex-direction: column;
  gap: 1.1rem;
  padding: 0.25rem 0;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}

.field > label {
  font-size: 0.875rem;
  font-weight: 500;
}

.row-2col {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
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
</style>
