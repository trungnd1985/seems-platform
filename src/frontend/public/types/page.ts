export interface SeoMeta {
  title: string
  description?: string
  ogTitle?: string
  ogDescription?: string
  ogImage?: string
  canonical?: string
  robots?: string
}

export interface SlotMapping {
  slotKey: string
  targetType: 'Content' | 'Module'
  targetId: string
  order: number
  parameters?: Record<string, unknown> | null
}

export interface Page {
  id: string
  slug: string
  title: string
  templateKey: string
  themeKey?: string
  seo: SeoMeta
  slots: SlotMapping[]
  status: 'draft' | 'published'
  createdAt: string
  updatedAt: string
  /** Populated when the page was matched via a parametric path pattern (e.g. blog/:id). */
  urlParameters?: Record<string, string> | null
}
