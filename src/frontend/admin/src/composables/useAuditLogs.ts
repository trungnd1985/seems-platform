import { ref } from 'vue'
import { useApi } from '@/composables/useApi'
import type { AuditLog, AuditLogFilters, PaginatedAuditLogs } from '@/types/auditLog'

export function useAuditLogs() {
  const api = useApi()
  const logs = ref<AuditLog[]>([])
  const total = ref(0)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchLogs(filters: AuditLogFilters, page = 1, pageSize = 50) {
    loading.value = true
    error.value = null
    try {
      const params: Record<string, string | number | null | undefined> = {
        page,
        pageSize,
        entityName: filters.entityName || undefined,
        action: filters.action || undefined,
        userEmail: filters.userEmail || undefined,
        dateFrom: filters.dateFrom?.toISOString() ?? undefined,
        dateTo: filters.dateTo?.toISOString() ?? undefined,
      }
      const { data } = await api.get<PaginatedAuditLogs>('/audit-logs', { params })
      logs.value = data.items
      total.value = data.total
    } catch (e: unknown) {
      const err = e as { response?: { data?: { message?: string } } }
      error.value = err.response?.data?.message ?? 'Failed to load audit logs.'
    } finally {
      loading.value = false
    }
  }

  return { logs, total, loading, error, fetchLogs }
}
