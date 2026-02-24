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
}
