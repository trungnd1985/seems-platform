import { ref } from 'vue'
import { useApi } from '@/composables/useApi'
import type {
  ContentItem,
  ContentStatus,
  CreateContentItemRequest,
  UpdateContentItemRequest,
} from '@/types/contentTypes'

interface PaginatedList<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

interface ListContentItemsParams {
  page?: number
  pageSize?: number
  contentTypeKey?: string
  status?: ContentStatus | ''
  categoryId?: string
  search?: string
}

export function useContentItems() {
  const api = useApi()
  const items = ref<ContentItem[]>([])
  const totalCount = ref(0)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchContentItems(params: ListContentItemsParams = {}) {
    loading.value = true
    error.value = null
    try {
      const query: Record<string, string | number> = {
        page: params.page ?? 1,
        pageSize: params.pageSize ?? 20,
      }
      if (params.contentTypeKey) query.contentTypeKey = params.contentTypeKey
      if (params.status) query.status = params.status
      if (params.categoryId) query.categoryId = params.categoryId
      if (params.search && params.search.length >= 3) query.search = params.search

      const { data } = await api.get<PaginatedList<ContentItem>>('/content-items', { params: query })
      items.value = data.items
      totalCount.value = data.total
    } catch (e: any) {
      error.value = e.response?.data?.message ?? 'Failed to load content items.'
    } finally {
      loading.value = false
    }
  }

  async function createContentItem(payload: CreateContentItemRequest): Promise<ContentItem> {
    const { data } = await api.post<ContentItem>('/content-items', payload)
    return data
  }

  async function updateContentItem(id: string, payload: UpdateContentItemRequest): Promise<ContentItem> {
    const { data } = await api.put<ContentItem>(`/content-items/${id}`, payload)
    return data
  }

  async function deleteContentItem(id: string): Promise<void> {
    await api.delete(`/content-items/${id}`)
  }

  return {
    items,
    totalCount,
    loading,
    error,
    fetchContentItems,
    createContentItem,
    updateContentItem,
    deleteContentItem,
  }
}
