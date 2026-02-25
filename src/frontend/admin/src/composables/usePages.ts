import { ref } from 'vue'
import { useApi } from '@/composables/useApi'
import type {
  Page,
  PageTreeNode,
  PaginatedPages,
  CreatePageRequest,
  UpdatePageRequest,
  AddSlotRequest,
  SlotMapping,
  SlotOrderItem,
  PageStatus,
} from '@/types/pages'

function buildTree(pages: Page[], parentId: string | null = null): PageTreeNode[] {
  return pages
    .filter((p) => p.parentId === parentId)
    .sort((a, b) => a.sortOrder - b.sortOrder || a.title.localeCompare(b.title))
    .map((p) => ({
      key: p.id,
      data: p,
      children: buildTree(pages, p.id),
    }))
}

export function usePages() {
  const api = useApi()
  const pages = ref<Page[]>([])
  const treeNodes = ref<PageTreeNode[]>([])
  const total = ref(0)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchPages(page = 1, pageSize = 20) {
    loading.value = true
    error.value = null
    try {
      const { data } = await api.get<PaginatedPages>('/pages', { params: { page, pageSize } })
      pages.value = data.items
      total.value = data.total
    } catch (e: any) {
      error.value = e.response?.data?.message ?? 'Failed to load pages.'
    } finally {
      loading.value = false
    }
  }

  async function fetchPageTree() {
    loading.value = true
    error.value = null
    try {
      const { data } = await api.get<Page[]>('/pages/tree')
      pages.value = data
      treeNodes.value = buildTree(data)
    } catch (e: any) {
      error.value = e.response?.data?.message ?? 'Failed to load pages.'
    } finally {
      loading.value = false
    }
  }

  async function getPage(id: string): Promise<Page> {
    const { data } = await api.get<Page>(`/pages/${id}`)
    return data
  }

  async function createPage(payload: CreatePageRequest): Promise<Page> {
    const { data } = await api.post<Page>('/pages', payload)
    return data
  }

  async function updatePage(id: string, payload: UpdatePageRequest): Promise<Page> {
    const { data } = await api.put<Page>(`/pages/${id}`, payload)
    return data
  }

  async function deletePage(id: string): Promise<void> {
    await api.delete(`/pages/${id}`)
  }

  async function updatePageStatus(id: string, status: PageStatus): Promise<Page> {
    const { data } = await api.patch<Page>(`/pages/${id}/status`, { status })
    return data
  }

  async function addSlot(payload: AddSlotRequest): Promise<SlotMapping> {
    const { data } = await api.post<SlotMapping>(`/pages/${payload.pageId}/slots`, payload)
    return data
  }

  async function removeSlot(pageId: string, slotId: string): Promise<void> {
    await api.delete(`/pages/${pageId}/slots/${slotId}`)
  }

  async function reorderSlots(pageId: string, items: SlotOrderItem[]): Promise<void> {
    await api.patch(`/pages/${pageId}/slots/order`, items)
  }

  async function setDefaultPage(id: string): Promise<Page> {
    const { data } = await api.patch<Page>(`/pages/${id}/set-default`)
    return data
  }

  return {
    pages,
    treeNodes,
    total,
    loading,
    error,
    fetchPages,
    fetchPageTree,
    getPage,
    createPage,
    updatePage,
    deletePage,
    updatePageStatus,
    addSlot,
    removeSlot,
    reorderSlots,
    setDefaultPage,
  }
}
