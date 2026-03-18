<script setup lang="ts">
import { ref, computed, reactive } from 'vue'

// ── Types ─────────────────────────────────────────────────────────────────

interface FieldConfig {
  name: string
  label: string
  /** text | email | tel | number | textarea | select */
  type: string
  required?: boolean
  placeholder?: string
  /** Options for select fields */
  options?: string[]
  /** Number of columns this field spans (1 or 2). Default: auto based on type */
  span?: 1 | 2
  /** Set to false to hide this field without removing it from the config. Defaults to true. */
  enabled?: boolean
}

interface ContactFormParams {
  fields?: FieldConfig[]
  submitLabel?: string
  successMessage?: string
  /** Page slug forwarded with the submission for server-side logging */
  pageSlug?: string
}

// ── Props ─────────────────────────────────────────────────────────────────

const props = defineProps<{
  moduleKey: string
  parameters?: Record<string, unknown> | null
}>()

// ── Parsed config ─────────────────────────────────────────────────────────

const DEFAULT_FIELDS: FieldConfig[] = [
  { name: 'name',    label: 'Full Name',      type: 'text',     required: true,  placeholder: 'Your full name', span: 1 },
  { name: 'email',   label: 'Email Address',  type: 'email',    required: true,  placeholder: 'your@email.com', span: 1 },
  { name: 'subject', label: 'Subject',        type: 'text',     required: true,  placeholder: 'How can we help you?' },
  { name: 'message', label: 'Message',        type: 'textarea', required: true,  placeholder: 'Write your message here…' },
]

const params = computed<ContactFormParams>(() =>
  (props.parameters ?? {}) as ContactFormParams,
)

const fields = computed<FieldConfig[]>(() => {
  const source = params.value.fields?.length ? params.value.fields : DEFAULT_FIELDS
  return source.filter(f => f.enabled !== false)
})

const submitLabel = computed(() => params.value.submitLabel ?? 'Send Message')
const successMessage = computed(() =>
  params.value.successMessage ?? 'Your message has been sent successfully. We\'ll be in touch soon!',
)

// ── Group fields into rows (pair single-span fields side by side) ─────────

interface FieldRow {
  fields: FieldConfig[]
  full: boolean // true = textarea / select / span:2
}

const fieldRows = computed<FieldRow[]>(() => {
  const rows: FieldRow[] = []
  let i = 0
  while (i < fields.value.length) {
    const f = fields.value[i]
    const isFullWidth = f.type === 'textarea' || f.type === 'select' || f.span === 2
    if (isFullWidth) {
      rows.push({ fields: [f], full: true })
      i++
    } else {
      const next = fields.value[i + 1]
      const nextIsFull = !next || next.type === 'textarea' || next.type === 'select' || next.span === 2
      if (next && !nextIsFull) {
        rows.push({ fields: [f, next], full: false })
        i += 2
      } else {
        rows.push({ fields: [f], full: true })
        i++
      }
    }
  }
  return rows
})

// ── Form state ────────────────────────────────────────────────────────────

type Status = 'idle' | 'sending' | 'success' | 'error'
const status = ref<Status>('idle')
const serverError = ref('')

const values = reactive<Record<string, string>>({})
const errors = reactive<Record<string, string>>({})

// ── Validation ────────────────────────────────────────────────────────────

function validate(): boolean {
  let valid = true
  for (const field of fields.value) {
    const val = (values[field.name] ?? '').trim()

    if (field.required && !val) {
      errors[field.name] = `${field.label} is required.`
      valid = false
    } else if (field.type === 'email' && val && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val)) {
      errors[field.name] = 'Please enter a valid email address.'
      valid = false
    } else if (field.type === 'tel' && val && !/^[+\d\s\-().]{7,20}$/.test(val)) {
      errors[field.name] = 'Please enter a valid phone number.'
      valid = false
    } else {
      errors[field.name] = ''
    }
  }
  return valid
}

function clearError(name: string) {
  errors[name] = ''
}

// ── Submit ────────────────────────────────────────────────────────────────

async function submit() {
  if (!validate()) return

  status.value = 'sending'
  serverError.value = ''

  try {
    const res = await fetch('/api/modules/contact-form/submit', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        pageSlug: params.value.pageSlug ?? null,
        fields: { ...values },
        // Echo field config so the server can enforce required-field validation
        fieldConfig: fields.value.map(f => ({
          name: f.name,
          label: f.label,
          type: f.type,
          required: f.required ?? false,
        })),
      }),
    })

    if (res.ok) {
      status.value = 'success'
    } else {
      const body = await res.json().catch(() => ({}))
      serverError.value = body.message ?? 'Something went wrong. Please try again.'
      status.value = 'error'
    }
  } catch {
    serverError.value = 'Network error. Please check your connection and try again.'
    status.value = 'error'
  }
}

function tryAgain() {
  status.value = 'idle'
  serverError.value = ''
}
</script>

<template>
  <div class="cf-wrap">
    <!-- ── Success state ── -->
    <div v-if="status === 'success'" class="cf-success" role="alert">
      <div class="cf-success-icon" aria-hidden="true">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="12" cy="12" r="10" />
          <polyline points="9 12 11.5 14.5 16 9.5" />
        </svg>
      </div>
      <p class="cf-success-msg">{{ successMessage }}</p>
    </div>

    <!-- ── Form ── -->
    <form v-else class="cf-form" novalidate @submit.prevent="submit">
      <!-- Server error banner -->
      <div v-if="status === 'error'" class="cf-banner cf-banner--error" role="alert">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" aria-hidden="true">
          <circle cx="12" cy="12" r="10" />
          <line x1="12" y1="8" x2="12" y2="12" />
          <line x1="12" y1="16" x2="12.01" y2="16" />
        </svg>
        <span>{{ serverError }}</span>
        <button type="button" class="cf-banner-close" aria-label="Dismiss" @click="tryAgain">×</button>
      </div>

      <!-- Field rows -->
      <div
        v-for="(row, ri) in fieldRows"
        :key="ri"
        class="cf-row"
        :class="{ 'cf-row--split': !row.full }"
      >
        <div
          v-for="field in row.fields"
          :key="field.name"
          class="cf-group"
        >
          <label :for="`cf-${field.name}`" class="cf-label">
            {{ field.label }}
            <span v-if="field.required" class="cf-required" aria-hidden="true">*</span>
          </label>

          <!-- Textarea -->
          <textarea
            v-if="field.type === 'textarea'"
            :id="`cf-${field.name}`"
            v-model="values[field.name]"
            class="cf-control cf-textarea"
            :class="{ 'cf-control--error': errors[field.name] }"
            :placeholder="field.placeholder"
            rows="5"
            :aria-describedby="errors[field.name] ? `cf-err-${field.name}` : undefined"
            :aria-invalid="!!errors[field.name]"
            @input="clearError(field.name)"
          />

          <!-- Select -->
          <select
            v-else-if="field.type === 'select'"
            :id="`cf-${field.name}`"
            v-model="values[field.name]"
            class="cf-control cf-select"
            :class="{ 'cf-control--error': errors[field.name] }"
            :aria-describedby="errors[field.name] ? `cf-err-${field.name}` : undefined"
            :aria-invalid="!!errors[field.name]"
            @change="clearError(field.name)"
          >
            <option value="" disabled>{{ field.placeholder || `Select ${field.label}` }}</option>
            <option v-for="opt in field.options" :key="opt" :value="opt">{{ opt }}</option>
          </select>

          <!-- Input (text / email / tel / number / etc.) -->
          <input
            v-else
            :id="`cf-${field.name}`"
            v-model="values[field.name]"
            :type="field.type"
            class="cf-control"
            :class="{ 'cf-control--error': errors[field.name] }"
            :placeholder="field.placeholder"
            :autocomplete="field.type === 'email' ? 'email' : field.type === 'tel' ? 'tel' : undefined"
            :aria-describedby="errors[field.name] ? `cf-err-${field.name}` : undefined"
            :aria-invalid="!!errors[field.name]"
            @input="clearError(field.name)"
          >

          <span
            v-if="errors[field.name]"
            :id="`cf-err-${field.name}`"
            class="cf-error"
            role="alert"
          >
            {{ errors[field.name] }}
          </span>
        </div>
      </div>

      <!-- Submit -->
      <div class="cf-actions">
        <button type="submit" class="cf-btn" :disabled="status === 'sending'">
          <svg v-if="status === 'sending'" class="cf-spinner" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" aria-hidden="true">
            <path d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z" stroke-opacity="0.2" />
            <path d="M21 12a9 9 0 00-9-9" />
          </svg>
          {{ status === 'sending' ? 'Sending…' : submitLabel }}
        </button>
      </div>
    </form>
  </div>
</template>

<style scoped>
/* Uses platform CSS variables from main.css where available,
   with hard-coded fallbacks so the module works stand-alone too. */

.cf-wrap {
  width: 100%;
}

/* ── Success ── */
.cf-success {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: 40px 20px;
  gap: 1rem;
  background: #f0fdf4;
  border: 1px solid #bbf7d0;
  border-radius: 4px;
}

.cf-success-icon {
  width: 52px;
  height: 52px;
  color: #16a34a;
}

.cf-success-icon svg {
  width: 100%;
  height: 100%;
}

.cf-success-msg {
  font-size: 0.95rem;
  color: #166534;
  font-weight: 500;
  margin: 0;
  max-width: 400px;
}

/* ── Form layout ── */
.cf-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

/* ── Banner ── */
.cf-banner {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  padding: 12px 14px;
  border-radius: 3px;
  font-size: 0.82rem;
  line-height: 1.5;
}

.cf-banner--error {
  background: #fef2f2;
  border: 1px solid #fecaca;
  color: #991b1b;
}

.cf-banner svg { flex-shrink: 0; margin-top: 1px; }

.cf-banner-close {
  margin-left: auto;
  background: none;
  border: none;
  cursor: pointer;
  font-size: 1.1rem;
  color: inherit;
  line-height: 1;
  padding: 0 2px;
  flex-shrink: 0;
  opacity: 0.7;
}

.cf-banner-close:hover { opacity: 1; }

/* ── Row ── */
.cf-row {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1rem;
}

.cf-row--split {
  grid-template-columns: 1fr 1fr;
}

/* ── Field group ── */
.cf-group {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

/* ── Label ── */
.cf-label {
  font-size: 0.72rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #555;
}

.cf-required {
  color: #e05252;
  margin-left: 2px;
}

/* ── Controls ── */
.cf-control {
  padding: 10px 14px;
  border: 1px solid #ddd;
  border-radius: 3px;
  font-family: inherit;
  font-size: 0.875rem;
  color: #333;
  background: #fff;
  outline: none;
  width: 100%;
  transition: border-color 0.2s, box-shadow 0.2s;
  appearance: none;
}

.cf-control:focus {
  border-color: #0088cc;
  box-shadow: 0 0 0 3px rgba(0, 136, 204, 0.12);
}

.cf-control--error {
  border-color: #e05252;
}

.cf-control--error:focus {
  box-shadow: 0 0 0 3px rgba(224, 82, 82, 0.1);
}

.cf-textarea {
  resize: vertical;
  min-height: 110px;
}

.cf-select {
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='10' height='6' viewBox='0 0 10 6'%3E%3Cpath d='M1 1l4 4 4-4' stroke='%23999' stroke-width='1.5' fill='none' stroke-linecap='round'/%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 12px center;
  padding-right: 32px;
  cursor: pointer;
}

/* ── Inline error ── */
.cf-error {
  font-size: 0.72rem;
  color: #e05252;
  font-weight: 600;
}

/* ── Submit ── */
.cf-actions {
  padding-top: 4px;
}

.cf-btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 11px 28px;
  background: #0088cc;
  border: none;
  border-radius: 3px;
  color: #fff;
  font-family: inherit;
  font-size: 0.8rem;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  cursor: pointer;
  transition: background 0.2s;
}

.cf-btn:hover:not(:disabled) { background: #006fa3; }

.cf-btn:disabled {
  opacity: 0.65;
  cursor: not-allowed;
}

/* Spinner */
@keyframes cf-spin {
  to { transform: rotate(360deg); }
}

.cf-spinner {
  width: 15px;
  height: 15px;
  animation: cf-spin 0.75s linear infinite;
  flex-shrink: 0;
}

/* ── Responsive ── */
@media (max-width: 540px) {
  .cf-row--split {
    grid-template-columns: 1fr;
  }
}
</style>
