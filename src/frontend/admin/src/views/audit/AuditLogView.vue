<script setup lang="ts">
import { ref, onMounted } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Select from 'primevue/select'
import InputText from 'primevue/inputtext'
import DatePicker from 'primevue/datepicker'
import Button from 'primevue/button'
import Paginator from 'primevue/paginator'
import Dialog from 'primevue/dialog'
import { useAuditLogs } from '@/composables/useAuditLogs'
import type { AuditLog, AuditLogFilters } from '@/types/auditLog'

const ENTITY_NAMES = [
  'Page',
  'ContentItem',
  'ContentType',
  'Template',
  'Theme',
  'Module',
  'AppUser',
  'AppRole',
]

const ACTION_OPTIONS = [
  { label: 'All Actions', value: null },
  { label: 'Created', value: 'Created' },
  { label: 'Updated', value: 'Updated' },
  { label: 'Deleted', value: 'Deleted' },
]

const { logs, total, loading, error, fetchLogs } = useAuditLogs()

const PAGE_SIZE = 50
const page = ref(1)

const filters = ref<AuditLogFilters>({
  entityName: null,
  action: null,
  userEmail: null,
  dateFrom: null,
  dateTo: null,
})

// Detail dialog
const detailVisible = ref(false)
const selectedLog = ref<AuditLog | null>(null)
const idCopied = ref(false)

function openDetail(log: AuditLog) {
  selectedLog.value = log
  detailVisible.value = true
  idCopied.value = false
}

function closeDetail() {
  detailVisible.value = false
  selectedLog.value = null
}

async function copyEntityId() {
  if (!selectedLog.value) return
  await navigator.clipboard.writeText(selectedLog.value.entityId)
  idCopied.value = true
  setTimeout(() => {
    idCopied.value = false
  }, 2000)
}

async function load() {
  await fetchLogs(filters.value, page.value, PAGE_SIZE)
}

function onPageChange(event: { page: number }) {
  page.value = event.page + 1
  void load()
}

async function applyFilters() {
  page.value = 1
  await load()
}

function clearFilters() {
  filters.value = { entityName: null, action: null, userEmail: null, dateFrom: null, dateTo: null }
  page.value = 1
  void load()
}

function actionSeverity(action: string): 'success' | 'warn' | 'danger' | 'secondary' {
  switch (action) {
    case 'Created':
      return 'success'
    case 'Updated':
      return 'warn'
    case 'Deleted':
      return 'danger'
    default:
      return 'secondary'
  }
}

function formatTimestamp(ts: string): string {
  return new Date(ts).toLocaleString()
}

function truncateId(id: string): string {
  return id.length > 8 ? id.slice(0, 8) + '…' : id
}

onMounted(load)
</script>

<template>
  <div class="audit-page">
    <div class="page-header">
      <div>
        <h1 class="page-title">Audit Log</h1>
        <p class="page-subtitle">Track all changes made to content, structure, and user accounts.</p>
      </div>
    </div>

    <!-- Filter bar -->
    <div class="filter-bar">
      <Select
        :model-value="filters.entityName"
        @update:model-value="filters.entityName = $event"
        :options="ENTITY_NAMES"
        placeholder="All Entities"
        show-clear
        class="filter-select"
      />

      <Select
        :model-value="filters.action"
        @update:model-value="filters.action = $event"
        :options="ACTION_OPTIONS"
        option-label="label"
        option-value="value"
        placeholder="All Actions"
        class="filter-select"
      />

      <InputText
        v-model="filters.userEmail"
        placeholder="Filter by user email"
        class="filter-input"
      />

      <DatePicker
        :model-value="filters.dateFrom"
        @update:model-value="filters.dateFrom = $event"
        placeholder="From date"
        date-format="yy-mm-dd"
        show-icon
        class="filter-date"
      />

      <DatePicker
        :model-value="filters.dateTo"
        @update:model-value="filters.dateTo = $event"
        placeholder="To date"
        date-format="yy-mm-dd"
        show-icon
        class="filter-date"
      />

      <Button label="Apply" icon="pi pi-search" @click="applyFilters" />
      <Button label="Clear" icon="pi pi-times" severity="secondary" text @click="clearFilters" />
    </div>

    <div v-if="error" class="error-banner">{{ error }}</div>

    <DataTable
      :value="logs"
      :loading="loading"
      striped-rows
      class="audit-table"
    >
      <Column header="Timestamp" style="width: 175px">
        <template #body="{ data }">
          <span class="timestamp">{{ formatTimestamp(data.timestamp) }}</span>
        </template>
      </Column>

      <Column field="entityName" header="Entity" style="width: 130px" />

      <Column header="Entity ID" style="width: 115px">
        <template #body="{ data }">
          <code class="entity-id" :title="data.entityId">{{ truncateId(data.entityId) }}</code>
        </template>
      </Column>

      <Column header="Action" style="width: 100px">
        <template #body="{ data }">
          <Tag :value="data.action" :severity="actionSeverity(data.action)" />
        </template>
      </Column>

      <Column header="User">
        <template #body="{ data }">
          <span class="user-email">{{ data.userEmail ?? '—' }}</span>
        </template>
      </Column>

      <Column header="Changed Fields">
        <template #body="{ data }">
          <span v-if="data.changedFields?.length" class="changed-fields">
            {{ data.changedFields.join(', ') }}
          </span>
          <span v-else class="text-muted">—</span>
        </template>
      </Column>

      <Column style="width: 60px">
        <template #body="{ data }">
          <Button
            icon="pi pi-eye"
            text
            rounded
            severity="secondary"
            aria-label="View detail"
            @click="openDetail(data)"
          />
        </template>
      </Column>
    </DataTable>

    <Paginator
      v-if="total > PAGE_SIZE"
      :rows="PAGE_SIZE"
      :total-records="total"
      :first="(page - 1) * PAGE_SIZE"
      @page="onPageChange"
      class="audit-paginator"
    />

    <!-- Detail dialog -->
    <Dialog
      :visible="detailVisible"
      @update:visible="closeDetail"
      header="Audit Log Detail"
      :style="{ width: '480px' }"
      modal
    >
      <div v-if="selectedLog" class="detail-body">
        <div class="detail-row">
          <span class="detail-label">Log ID</span>
          <span class="detail-value">#{{ selectedLog.id }}</span>
        </div>

        <div class="detail-row">
          <span class="detail-label">Timestamp</span>
          <span class="detail-value">{{ formatTimestamp(selectedLog.timestamp) }}</span>
        </div>

        <div class="detail-row">
          <span class="detail-label">Action</span>
          <Tag :value="selectedLog.action" :severity="actionSeverity(selectedLog.action)" />
        </div>

        <div class="detail-row">
          <span class="detail-label">Entity</span>
          <span class="detail-value">{{ selectedLog.entityName }}</span>
        </div>

        <div class="detail-row">
          <span class="detail-label">Entity ID</span>
          <div class="id-row">
            <code class="entity-id-full">{{ selectedLog.entityId }}</code>
            <Button
              :icon="idCopied ? 'pi pi-check' : 'pi pi-copy'"
              text
              rounded
              size="small"
              :severity="idCopied ? 'success' : 'secondary'"
              v-tooltip="idCopied ? 'Copied!' : 'Copy ID'"
              aria-label="Copy entity ID"
              @click="copyEntityId"
            />
          </div>
        </div>

        <div class="detail-row">
          <span class="detail-label">User</span>
          <span class="detail-value">{{ selectedLog.userEmail ?? '—' }}</span>
        </div>

        <div v-if="selectedLog.userId" class="detail-row">
          <span class="detail-label">User ID</span>
          <code class="entity-id-full">{{ selectedLog.userId }}</code>
        </div>

        <div class="detail-row detail-row-top">
          <span class="detail-label">Changed Fields</span>
          <div v-if="selectedLog.changedFields?.length" class="fields-list">
            <Tag
              v-for="field in selectedLog.changedFields"
              :key="field"
              :value="field"
              severity="secondary"
              class="field-tag"
            />
          </div>
          <span v-else class="text-muted">—</span>
        </div>
      </div>

      <template #footer>
        <Button label="Close" @click="closeDetail" />
      </template>
    </Dialog>
  </div>
</template>

<style scoped>
.audit-page {
  padding: 1.5rem;
}

.page-header {
  margin-bottom: 1.25rem;
}

.page-title {
  font-size: 1.5rem;
  font-weight: 600;
  margin: 0 0 0.25rem;
}

.page-subtitle {
  margin: 0;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.filter-bar {
  display: flex;
  flex-wrap: wrap;
  gap: 0.625rem;
  margin-bottom: 1.25rem;
  align-items: center;
}

.filter-select {
  width: 160px;
}

.filter-input {
  width: 200px;
}

.filter-date {
  width: 160px;
}

.error-banner {
  background: var(--p-red-50);
  color: var(--p-red-700);
  border: 1px solid var(--p-red-200);
  border-radius: 6px;
  padding: 0.75rem 1rem;
  margin-bottom: 1rem;
  font-size: 0.875rem;
}

.timestamp {
  font-size: 0.8125rem;
  white-space: nowrap;
}

.entity-id {
  font-family: monospace;
  font-size: 0.8125rem;
  background: var(--p-surface-100);
  padding: 0.125rem 0.375rem;
  border-radius: 4px;
  cursor: default;
}

.user-email {
  font-size: 0.875rem;
}

.changed-fields {
  font-size: 0.8125rem;
  color: var(--p-text-muted-color);
}

.text-muted {
  color: var(--p-text-muted-color);
}

.audit-paginator {
  margin-top: 1rem;
}

/* Detail dialog */
.detail-body {
  display: flex;
  flex-direction: column;
  gap: 0.875rem;
  padding: 0.25rem 0;
}

.detail-row {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.detail-row-top {
  align-items: flex-start;
}

.detail-label {
  font-size: 0.8125rem;
  font-weight: 600;
  color: var(--p-text-muted-color);
  width: 110px;
  flex-shrink: 0;
}

.detail-value {
  font-size: 0.875rem;
}

.id-row {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  min-width: 0;
}

.entity-id-full {
  font-family: monospace;
  font-size: 0.8125rem;
  background: var(--p-surface-100);
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
  word-break: break-all;
}

.fields-list {
  display: flex;
  flex-wrap: wrap;
  gap: 0.375rem;
}

.field-tag {
  font-family: monospace;
  font-size: 0.75rem;
}
</style>
