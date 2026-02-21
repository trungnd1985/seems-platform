export type PageStatus = 'Draft' | 'Published' | 'Archived'
export type SlotTargetType = 'Content' | 'Module'

export interface SeoMeta {
  title: string
  description?: string | null
  ogTitle?: string | null
  ogDescription?: string | null
  ogImage?: string | null
  canonical?: string | null
  robots?: string | null
}

export interface SlotMapping {
  id: string
  slotKey: string
  targetType: SlotTargetType
  targetId: string
  order: number
}

export interface Page {
  id: string
  slug: string
  title: string
  templateKey: string
  themeKey: string | null
  seo: SeoMeta
  status: PageStatus
  isDefault: boolean
  slots: SlotMapping[]
  createdAt: string
  updatedAt: string
}

export interface PaginatedPages {
  items: Page[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

export interface CreatePageRequest {
  slug: string
  title: string
  templateKey: string
  themeKey?: string | null
  seo?: SeoMeta | null
}

export interface UpdatePageRequest {
  id: string
  slug: string
  title: string
  templateKey: string
  themeKey?: string | null
  seo?: SeoMeta | null
}

export interface AddSlotRequest {
  pageId: string
  slotKey: string
  targetType: SlotTargetType
  targetId: string
}

export interface SlotOrderItem {
  slotId: string
  order: number
}

export const PAGE_STATUS_SEVERITIES: Record<PageStatus, string> = {
  Draft: 'secondary',
  Published: 'success',
  Archived: 'warn',
}

export const SLOT_TARGET_TYPES: { value: SlotTargetType; label: string }[] = [
  { value: 'Content', label: 'Content Item' },
  { value: 'Module', label: 'Module' },
]
