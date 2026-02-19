export type ContentStatus = 'draft' | 'published' | 'archived'

export interface ContentType {
  id: string
  key: string
  name: string
  schema: Record<string, unknown>
}

export interface ContentItem {
  id: string
  contentTypeKey: string
  data: Record<string, unknown>
  status: ContentStatus
  createdAt: string
  updatedAt: string
}
