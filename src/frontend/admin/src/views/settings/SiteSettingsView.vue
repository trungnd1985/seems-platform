<script setup lang="ts">
import { ref, onMounted } from 'vue'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
import Select from 'primevue/select'
import InputText from 'primevue/inputtext'
import Button from 'primevue/button'
import Message from 'primevue/message'
import Toast from 'primevue/toast'
import { useToast } from 'primevue/usetoast'
import { useMedia } from '@/composables/useMedia'
import type { StorageSettings, SiteInfo } from '@/types/media'

const {
  getStorageSettings,
  updateStorageSettings,
  getSiteInfo,
  updateSiteInfo,
  uploadMedia,
} = useMedia()
const toast = useToast()

const loading = ref(true)
const savingGeneral = ref(false)
const savingStorage = ref(false)

// --- General (site info) ---
const general = ref<SiteInfo>({
  siteName: '',
  tagline: '',
  logoMediaId: null,
  logoUrl: null,
  faviconMediaId: null,
  faviconUrl: null,
})
const logoUploading = ref(false)
const faviconUploading = ref(false)
const logoInput = ref<HTMLInputElement | null>(null)
const faviconInput = ref<HTMLInputElement | null>(null)

async function onLogoFileChange(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0]
  if (!file) return
  logoUploading.value = true
  try {
    const media = await uploadMedia(file)
    general.value.logoMediaId = media.id
    general.value.logoUrl = media.url
  } catch {
    toast.add({ severity: 'error', summary: 'Error', detail: 'Logo upload failed', life: 4000 })
  } finally {
    logoUploading.value = false
    if (logoInput.value) logoInput.value.value = ''
  }
}

function clearLogo() {
  general.value.logoMediaId = null
  general.value.logoUrl = null
}

async function onFaviconFileChange(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0]
  if (!file) return
  faviconUploading.value = true
  try {
    const media = await uploadMedia(file)
    general.value.faviconMediaId = media.id
    general.value.faviconUrl = media.url
  } catch {
    toast.add({ severity: 'error', summary: 'Error', detail: 'Favicon upload failed', life: 4000 })
  } finally {
    faviconUploading.value = false
    if (faviconInput.value) faviconInput.value.value = ''
  }
}

function clearFavicon() {
  general.value.faviconMediaId = null
  general.value.faviconUrl = null
}

async function saveGeneral() {
  savingGeneral.value = true
  try {
    await updateSiteInfo(general.value)
    toast.add({ severity: 'success', summary: 'Saved', detail: 'Site information updated', life: 3000 })
  } catch {
    toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to save site information', life: 4000 })
  } finally {
    savingGeneral.value = false
  }
}

// --- Storage ---
const storage = ref<StorageSettings>({
  provider: 'local',
  local: { baseUrl: 'http://localhost:5000' },
  s3: { bucketName: '', region: '', serviceUrl: '', accessKey: '', secretKey: '' },
})

const providerOptions = [
  { label: 'Local filesystem', value: 'local' },
  { label: 'Amazon S3 / S3-compatible', value: 's3' },
]

async function saveStorage() {
  savingStorage.value = true
  try {
    await updateStorageSettings(storage.value)
    toast.add({ severity: 'success', summary: 'Saved', detail: 'Storage settings updated', life: 3000 })
  } catch {
    toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to save settings', life: 4000 })
  } finally {
    savingStorage.value = false
  }
}

onMounted(async () => {
  try {
    const [siteInfo, storageSettings] = await Promise.all([getSiteInfo(), getStorageSettings()])
    general.value = siteInfo
    storage.value = storageSettings
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="settings-page">
    <Toast />

    <div class="page-header">
      <div>
        <h1 class="page-title">Site Settings</h1>
        <p class="page-subtitle">Configure global platform settings.</p>
      </div>
    </div>

    <div v-if="loading" class="flex justify-center py-16">
      <i class="pi pi-spin pi-spinner text-3xl text-muted-color" />
    </div>

    <Tabs v-else value="general">
      <TabList>
        <Tab value="general">General</Tab>
        <Tab value="storage">Storage</Tab>
      </TabList>

      <TabPanels>
        <!-- General / Site Info -->
        <TabPanel value="general">
          <div class="settings-card">
            <h2 class="settings-section-title">Site Information</h2>
            <p class="text-sm text-muted-color mb-4">
              Basic identity settings shown in the browser, admin interface, and public site.
            </p>

            <div class="field">
              <label class="field-label">Site Name</label>
              <InputText v-model="general.siteName" placeholder="My Website" class="w-full" />
            </div>

            <div class="field">
              <label class="field-label">Tagline</label>
              <InputText v-model="general.tagline" placeholder="Just another great site" class="w-full" />
              <small class="text-muted-color">Short description used in meta tags and the public header.</small>
            </div>

            <div class="field">
              <label class="field-label">Logo</label>
              <div class="image-picker">
                <div class="image-preview">
                  <img v-if="general.logoUrl" :src="general.logoUrl" alt="Logo" class="preview-img" />
                  <div v-else class="preview-placeholder">
                    <i class="pi pi-image" />
                  </div>
                </div>
                <div class="image-picker-actions">
                  <Button
                    label="Upload"
                    icon="pi pi-upload"
                    severity="secondary"
                    size="small"
                    :loading="logoUploading"
                    @click="logoInput?.click()"
                  />
                  <Button
                    v-if="general.logoMediaId"
                    label="Remove"
                    icon="pi pi-times"
                    severity="danger"
                    size="small"
                    text
                    @click="clearLogo"
                  />
                </div>
                <input ref="logoInput" type="file" accept="image/*" class="hidden-input" @change="onLogoFileChange" />
              </div>
              <small class="text-muted-color">Recommended: SVG or PNG with transparent background.</small>
            </div>

            <div class="field">
              <label class="field-label">Favicon</label>
              <div class="image-picker">
                <div class="image-preview image-preview--small">
                  <img v-if="general.faviconUrl" :src="general.faviconUrl" alt="Favicon" class="preview-img" />
                  <div v-else class="preview-placeholder">
                    <i class="pi pi-image" />
                  </div>
                </div>
                <div class="image-picker-actions">
                  <Button
                    label="Upload"
                    icon="pi pi-upload"
                    severity="secondary"
                    size="small"
                    :loading="faviconUploading"
                    @click="faviconInput?.click()"
                  />
                  <Button
                    v-if="general.faviconMediaId"
                    label="Remove"
                    icon="pi pi-times"
                    severity="danger"
                    size="small"
                    text
                    @click="clearFavicon"
                  />
                </div>
                <input ref="faviconInput" type="file" accept="image/x-icon,image/png,image/svg+xml" class="hidden-input" @change="onFaviconFileChange" />
              </div>
              <small class="text-muted-color">Recommended: 32×32 or 64×64 ICO/PNG.</small>
            </div>

            <div class="flex justify-end mt-4">
              <Button label="Save" icon="pi pi-check" :loading="savingGeneral" @click="saveGeneral" />
            </div>
          </div>
        </TabPanel>

        <!-- Storage -->
        <TabPanel value="storage">
          <div class="settings-card">
            <h2 class="settings-section-title">Storage Provider</h2>
            <p class="text-sm text-muted-color mb-4">
              Choose where uploaded media files are stored. Changing the provider does not migrate existing files.
            </p>

            <div class="field">
              <label class="field-label">Provider</label>
              <Select
                :model-value="storage.provider"
                @update:model-value="storage.provider = $event"
                :options="providerOptions"
                option-label="label"
                option-value="value"
                class="w-full"
              />
            </div>

            <template v-if="storage.provider === 'local'">
              <div class="field">
                <label class="field-label">Base URL</label>
                <InputText
                  v-model="storage.local.baseUrl"
                  placeholder="http://localhost:5000"
                  class="w-full"
                />
                <small class="text-muted-color">Public base URL used to build file URLs.</small>
              </div>
            </template>

            <template v-if="storage.provider === 's3'">
              <Message severity="warn" class="mb-4">
                The S3 provider is not yet implemented. Configuration will be saved but uploads will fail until the provider is developed.
              </Message>

              <div class="field">
                <label class="field-label">Bucket Name</label>
                <InputText v-model="storage.s3.bucketName" class="w-full" />
              </div>
              <div class="field">
                <label class="field-label">Region</label>
                <InputText v-model="storage.s3.region" placeholder="us-east-1" class="w-full" />
              </div>
              <div class="field">
                <label class="field-label">Service URL <span class="text-muted-color">(leave empty for AWS)</span></label>
                <InputText v-model="storage.s3.serviceUrl" placeholder="https://s3.example.com" class="w-full" />
              </div>
              <div class="field">
                <label class="field-label">Access Key</label>
                <InputText v-model="storage.s3.accessKey" class="w-full" />
              </div>
              <div class="field">
                <label class="field-label">Secret Key</label>
                <InputText v-model="storage.s3.secretKey" type="password" class="w-full" />
              </div>
            </template>

            <div class="flex justify-end mt-4">
              <Button label="Save" icon="pi pi-check" :loading="savingStorage" @click="saveStorage" />
            </div>
          </div>
        </TabPanel>
      </TabPanels>
    </Tabs>
  </div>
</template>

<style scoped>
.settings-page {
  max-width: 640px;
}
.page-header {
  margin-bottom: 1.5rem;
}
.page-title {
  font-size: 1.25rem;
  font-weight: 600;
  margin: 0;
}
.page-subtitle {
  color: var(--p-text-muted-color);
  margin: 0.25rem 0 0;
  font-size: 0.875rem;
}
.settings-card {
  background: var(--p-content-background);
  border: 1px solid var(--p-content-border-color);
  border-radius: var(--p-border-radius-lg);
  padding: 1.5rem;
  margin-top: 1rem;
}
.settings-section-title {
  font-size: 1rem;
  font-weight: 600;
  margin: 0 0 0.25rem;
}
.field {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
  margin-bottom: 1rem;
}
.field-label {
  font-size: 0.875rem;
  font-weight: 500;
}
.image-picker {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}
.image-preview {
  width: 80px;
  height: 80px;
  border: 1px solid var(--p-content-border-color);
  border-radius: var(--p-border-radius-md);
  overflow: hidden;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--p-surface-50);
}
.image-preview--small {
  width: 48px;
  height: 48px;
}
.preview-img {
  width: 100%;
  height: 100%;
  object-fit: contain;
}
.preview-placeholder {
  color: var(--p-text-muted-color);
  font-size: 1.5rem;
}
.image-picker-actions {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}
.hidden-input {
  display: none;
}
</style>
