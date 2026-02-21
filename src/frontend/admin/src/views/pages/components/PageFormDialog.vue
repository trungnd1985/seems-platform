<script setup lang="ts">
import { ref, reactive, computed, watch } from 'vue'
import Dialog from 'primevue/dialog'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Select from 'primevue/select'
import Button from 'primevue/button'
import PageSlotEditor from './PageSlotEditor.vue'
import type { Page, SeoMeta, SlotMapping } from '@/types/pages'
import type { Template } from '@/types/templates'
import type { Theme } from '@/types/themes'

const props = defineProps<{
  visible: boolean
  page: Page | null
  templates: Template[]
  themes: Theme[]
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  saved: [payload: { slug: string; title: string; templateKey: string; themeKey: string | null; seo: SeoMeta }]
}>()

const activeTab = ref('page')
const slugManuallyEdited = ref(false)

const form = reactive({
  slug: '',
  title: '',
  templateKey: '',
  themeKey: null as string | null,
  seo: {
    title: '',
    description: null as string | null,
    ogTitle: null as string | null,
    ogDescription: null as string | null,
    ogImage: null as string | null,
    canonical: null as string | null,
    robots: null as string | null,
  },
})

// Track local slots for the Slots tab (edit mode only)
const localSlots = ref<SlotMapping[]>([])

watch(
  () => props.page,
  (p) => {
    activeTab.value = 'page'
    slugManuallyEdited.value = false
    form.slug = p?.slug ?? ''
    form.title = p?.title ?? ''
    form.templateKey = p?.templateKey ?? ''
    form.themeKey = p?.themeKey ?? null
    form.seo.title = p?.seo?.title ?? ''
    form.seo.description = p?.seo?.description ?? null
    form.seo.ogTitle = p?.seo?.ogTitle ?? null
    form.seo.ogDescription = p?.seo?.ogDescription ?? null
    form.seo.ogImage = p?.seo?.ogImage ?? null
    form.seo.canonical = p?.seo?.canonical ?? null
    form.seo.robots = p?.seo?.robots ?? null
    localSlots.value = p ? [...p.slots] : []
  },
  { immediate: true },
)

const isEdit = () => props.page !== null

const themeOptions = computed(() =>
  props.themes.map((t) => ({ label: `${t.name} (${t.key})`, value: t.key })),
)

// Filter templates to only those belonging to the selected theme.
// When no theme is selected, show all templates.
const templateOptions = computed(() => {
  const source = form.themeKey
    ? props.templates.filter((t) => t.themeKey === form.themeKey)
    : props.templates
  return source.map((t) => ({ label: `${t.name} (${t.key})`, value: t.key }))
})

// When the theme changes, clear the template selection if it no longer belongs to the new theme.
watch(
  () => form.themeKey,
  (newThemeKey) => {
    if (!form.templateKey) return
    const selected = props.templates.find((t) => t.key === form.templateKey)
    if (selected && newThemeKey && selected.themeKey !== newThemeKey) {
      form.templateKey = ''
    }
  },
)

const selectedTemplateSlots = computed(() => {
  const tpl = props.templates.find((t) => t.key === form.templateKey)
  return tpl?.slots ?? []
})

function generateSlug(title: string): string {
  return title
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9\s/_-]/g, '')
    .replace(/\s+/g, '-')
    .replace(/-+/g, '-')
    .replace(/^-+|-+$/g, '')
}

watch(
  () => form.title,
  (newTitle) => {
    if (!isEdit() && !slugManuallyEdited.value) {
      form.slug = generateSlug(newTitle)
    }
  },
)

function onSlugInput() {
  slugManuallyEdited.value = true
}

const slugInvalid = computed(
  () => form.slug.length > 0 && !/^[a-z0-9][a-z0-9/_-]*$/.test(form.slug),
)

function canSubmit(): boolean {
  return !!(
    form.slug.trim() &&
    form.title.trim() &&
    form.themeKey &&
    form.templateKey &&
    !slugInvalid.value
  )
}

function close() {
  emit('update:visible', false)
}

function submit() {
  emit('saved', {
    slug: form.slug.trim(),
    title: form.title.trim(),
    templateKey: form.templateKey,
    themeKey: form.themeKey,
    seo: {
      title: form.seo.title.trim() || form.title.trim(),
      description: form.seo.description || null,
      ogTitle: form.seo.ogTitle || null,
      ogDescription: form.seo.ogDescription || null,
      ogImage: form.seo.ogImage || null,
      canonical: form.seo.canonical || null,
      robots: form.seo.robots || null,
    },
  })
}
</script>

<template>
  <Dialog
    :visible="visible"
    :header="isEdit() ? 'Edit Page' : 'New Page'"
    :style="{ width: '620px' }"
    modal
    @update:visible="close"
  >
    <Tabs :value="activeTab" @update:value="activeTab = $event">
      <TabList>
        <Tab value="page">Page</Tab>
        <Tab value="seo">SEO</Tab>
        <Tab v-if="isEdit()" value="slots">Slots</Tab>
      </TabList>

      <TabPanels>
        <!-- PAGE TAB -->
        <TabPanel value="page">
          <form class="dialog-form" @submit.prevent="submit">
            <div class="field">
              <label for="pg-title">Title <span class="required">*</span></label>
              <InputText
                id="pg-title"
                v-model="form.title"
                placeholder="e.g. About Us"
                maxlength="512"
                fluid
                autofocus
              />
            </div>

            <div class="field">
              <label for="pg-slug">Slug <span v-if="!page?.isDefault" class="required">*</span></label>
              <InputText
                id="pg-slug"
                v-model="form.slug"
                placeholder="e.g. about-us"
                maxlength="256"
                fluid
                :disabled="page?.isDefault"
                :invalid="slugInvalid"
                @input="onSlugInput"
              />
              <small v-if="page?.isDefault" class="hint info">
                The home page is resolved by the Default flag, not by slug.
              </small>
              <small v-else-if="slugInvalid" class="hint error">
                Slug must be lowercase and contain only letters, digits, hyphens, underscores, or
                forward slashes.
              </small>
              <small v-else class="hint">Auto-generated from title; you can override it.</small>
            </div>

            <div class="field">
              <label for="pg-theme">Theme <span class="required">*</span></label>
              <Select
                id="pg-theme"
                :modelValue="form.themeKey"
                @update:modelValue="form.themeKey = $event"
                :options="themeOptions"
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
              <label for="pg-template">Template <span class="required">*</span></label>
              <Select
                id="pg-template"
                :modelValue="form.templateKey"
                @update:modelValue="form.templateKey = $event"
                :options="templateOptions"
                option-label="label"
                option-value="value"
                placeholder="Select a template"
                :disabled="!form.themeKey"
                fluid
              />
              <small v-if="!form.themeKey" class="hint">Select a theme first.</small>
              <small v-else-if="templateOptions.length === 0" class="hint warn">
                No templates for this theme. Create a template under
                <strong>{{ form.themeKey }}</strong> first.
              </small>
            </div>
          </form>
        </TabPanel>

        <!-- SEO TAB -->
        <TabPanel value="seo">
          <form class="dialog-form" @submit.prevent="submit">
            <div class="field">
              <label for="seo-title">SEO Title</label>
              <InputText
                id="seo-title"
                v-model="form.seo.title"
                placeholder="Defaults to page title"
                maxlength="512"
                fluid
              />
            </div>

            <div class="field">
              <label for="seo-desc">Meta Description</label>
              <Textarea
                id="seo-desc"
                v-model="form.seo.description"
                placeholder="160 characters recommended"
                rows="3"
                maxlength="512"
                fluid
              />
            </div>

            <div class="field-row">
              <div class="field">
                <label for="og-title">OG Title</label>
                <InputText
                  id="og-title"
                  v-model="form.seo.ogTitle"
                  placeholder="Open Graph title"
                  maxlength="512"
                  fluid
                />
              </div>
              <div class="field">
                <label for="og-image">OG Image URL</label>
                <InputText
                  id="og-image"
                  v-model="form.seo.ogImage"
                  placeholder="https://..."
                  maxlength="1024"
                  fluid
                />
              </div>
            </div>

            <div class="field">
              <label for="og-desc">OG Description</label>
              <Textarea
                id="og-desc"
                v-model="form.seo.ogDescription"
                placeholder="Open Graph description"
                rows="2"
                maxlength="512"
                fluid
              />
            </div>

            <div class="field-row">
              <div class="field">
                <label for="canonical">Canonical URL</label>
                <InputText
                  id="canonical"
                  v-model="form.seo.canonical"
                  placeholder="Leave empty to use page URL"
                  maxlength="1024"
                  fluid
                />
              </div>
              <div class="field">
                <label for="robots">Robots</label>
                <InputText
                  id="robots"
                  v-model="form.seo.robots"
                  placeholder="e.g. noindex, nofollow"
                  maxlength="128"
                  fluid
                />
              </div>
            </div>
          </form>
        </TabPanel>

        <!-- SLOTS TAB (edit only) -->
        <TabPanel v-if="isEdit()" value="slots">
          <PageSlotEditor
            :page="page!"
            :template-slots="selectedTemplateSlots"
            :initial-slots="localSlots"
            @updated="localSlots = $event"
          />
        </TabPanel>
      </TabPanels>
    </Tabs>

    <template #footer>
      <Button label="Cancel" text @click="close" />
      <Button
        :label="isEdit() ? 'Save Changes' : 'Create Page'"
        :disabled="!canSubmit()"
        @click="submit"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.dialog-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
  padding: 1rem 0 0.25rem;
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

.field-row {
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

.hint.error {
  color: var(--p-red-500);
}

.hint.info {
  color: var(--p-blue-500);
}

.hint.warn {
  color: var(--p-orange-500);
}
</style>
