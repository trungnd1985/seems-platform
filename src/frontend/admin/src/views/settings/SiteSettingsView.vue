<script setup lang="ts">
import { ref, onMounted } from 'vue'
import Select from 'primevue/select'
import InputText from 'primevue/inputtext'
import Button from 'primevue/button'
import Message from 'primevue/message'
import Toast from 'primevue/toast'
import { useToast } from 'primevue/usetoast'
import { useMedia } from '@/composables/useMedia'
import type { StorageSettings } from '@/types/media'

const { getStorageSettings, updateStorageSettings } = useMedia()
const toast = useToast()

const loading = ref(true)
const saving = ref(false)

const settings = ref<StorageSettings>({
  provider: 'local',
  local: { baseUrl: 'http://localhost:5000' },
  s3: { bucketName: '', region: '', serviceUrl: '', accessKey: '', secretKey: '' },
})

const providerOptions = [
  { label: 'Local filesystem', value: 'local' },
  { label: 'Amazon S3 / S3-compatible', value: 's3' },
]

onMounted(async () => {
  try {
    settings.value = await getStorageSettings()
  } finally {
    loading.value = false
  }
})

async function save() {
  saving.value = true
  try {
    await updateStorageSettings(settings.value)
    toast.add({ severity: 'success', summary: 'Saved', detail: 'Storage settings updated', life: 3000 })
  } catch {
    toast.add({ severity: 'error', summary: 'Error', detail: 'Failed to save settings', life: 4000 })
  } finally {
    saving.value = false
  }
}
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

    <div v-else class="settings-card">
      <h2 class="settings-section-title">Storage Provider</h2>
      <p class="text-sm text-muted-color mb-4">
        Choose where uploaded media files are stored. Changing the provider does not migrate existing files.
      </p>

      <div class="field">
        <label class="field-label">Provider</label>
        <Select
          :model-value="settings.provider"
          @update:model-value="settings.provider = $event"
          :options="providerOptions"
          option-label="label"
          option-value="value"
          class="w-full"
        />
      </div>

      <!-- Local storage config -->
      <template v-if="settings.provider === 'local'">
        <div class="field">
          <label class="field-label">Base URL</label>
          <InputText
            v-model="settings.local.baseUrl"
            placeholder="http://localhost:5000"
            class="w-full"
          />
          <small class="text-muted-color">Public base URL used to build file URLs.</small>
        </div>
      </template>

      <!-- S3 config -->
      <template v-if="settings.provider === 's3'">
        <Message severity="warn" class="mb-4">
          The S3 provider is not yet implemented. Configuration will be saved but uploads will fail until the provider is developed.
        </Message>

        <div class="field">
          <label class="field-label">Bucket Name</label>
          <InputText v-model="settings.s3.bucketName" class="w-full" />
        </div>
        <div class="field">
          <label class="field-label">Region</label>
          <InputText v-model="settings.s3.region" placeholder="us-east-1" class="w-full" />
        </div>
        <div class="field">
          <label class="field-label">Service URL <span class="text-muted-color">(leave empty for AWS)</span></label>
          <InputText v-model="settings.s3.serviceUrl" placeholder="https://s3.example.com" class="w-full" />
        </div>
        <div class="field">
          <label class="field-label">Access Key</label>
          <InputText v-model="settings.s3.accessKey" class="w-full" />
        </div>
        <div class="field">
          <label class="field-label">Secret Key</label>
          <InputText v-model="settings.s3.secretKey" type="password" class="w-full" />
        </div>
      </template>

      <div class="flex justify-end mt-4">
        <Button label="Save" icon="pi pi-check" :loading="saving" @click="save" />
      </div>
    </div>
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
</style>
