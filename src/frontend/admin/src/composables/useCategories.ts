import { ref } from 'vue'
import { useApi } from '@/composables/useApi'
import type { Category, CreateCategoryRequest, UpdateCategoryRequest } from '@/types/categoryTypes'

export function useCategories() {
  const api = useApi()
  const categories = ref<Category[]>([])
  const tree = ref<Category[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchCategories(contentTypeKey: string): Promise<void> {
    loading.value = true
    error.value = null
    try {
      const { data } = await api.get<Category[]>('/categories', { params: { contentTypeKey } })
      categories.value = data
    } catch (e: any) {
      error.value = e.response?.data?.message ?? 'Failed to load categories.'
    } finally {
      loading.value = false
    }
  }

  async function fetchCategoryTree(contentTypeKey: string): Promise<Category[]> {
    const { data } = await api.get<Category[]>('/categories/tree', { params: { contentTypeKey } })
    tree.value = data
    return data
  }

  async function createCategory(payload: CreateCategoryRequest): Promise<Category> {
    const { data } = await api.post<Category>('/categories', payload)
    return data
  }

  async function updateCategory(id: string, payload: UpdateCategoryRequest): Promise<Category> {
    const { data } = await api.put<Category>(`/categories/${id}`, payload)
    return data
  }

  async function deleteCategory(id: string): Promise<void> {
    await api.delete(`/categories/${id}`)
  }

  return {
    categories,
    tree,
    loading,
    error,
    fetchCategories,
    fetchCategoryTree,
    createCategory,
    updateCategory,
    deleteCategory,
  }
}
