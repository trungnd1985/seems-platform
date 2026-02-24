export type AuditAction = 'Created' | 'Updated' | 'Deleted'

export interface AuditLog {
  id: number
  entityName: string
  entityId: string
  action: AuditAction
  userId: string | null
  userEmail: string | null
  changedFields: string[] | null
  timestamp: string
}

export interface PaginatedAuditLogs {
  items: AuditLog[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

export interface AuditLogFilters {
  entityName: string | null
  action: string | null
  userEmail: string | null
  dateFrom: Date | null
  dateTo: Date | null
}
