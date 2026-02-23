export interface Category {
  id: string
  name: string
  slug: string
  description?: string
  parentId: string | null
  contentTypeKey: string
  sortOrder: number
  itemCount: number
  children?: Category[]
  createdAt: string
  updatedAt: string
}

export interface CreateCategoryRequest {
  name: string
  slug?: string
  description?: string
  contentTypeKey: string
  parentId?: string | null
  sortOrder?: number
}

export interface UpdateCategoryRequest {
  name: string
  slug?: string
  description?: string
  parentId?: string | null
  sortOrder?: number
}
