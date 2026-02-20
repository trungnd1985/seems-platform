import { ref } from 'vue'
import { useApi } from '@/composables/useApi'
import type {
  ContentType,
  CreateContentTypeRequest,
  UpdateContentTypeRequest,
} from '@/types/contentTypes'

export function useContentTypes() {
  const api = useApi()
  const contentTypes = ref<ContentType[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchContentTypes() {
    loading.value = true
    error.value = null
    try {
      const { data } = await api.get<ContentType[]>('/content-types')
      contentTypes.value = data
    } catch (e: any) {
      error.value = e.response?.data?.message ?? 'Failed to load content types.'
    } finally {
      loading.value = false
    }
  }

  async function createContentType(payload: CreateContentTypeRequest): Promise<ContentType> {
    const { data } = await api.post<ContentType>('/content-types', payload)
    return data
  }

  async function updateContentType(id: string, payload: UpdateContentTypeRequest): Promise<ContentType> {
    const { data } = await api.put<ContentType>(`/content-types/${id}`, payload)
    return data
  }

  async function deleteContentType(id: string): Promise<void> {
    await api.delete(`/content-types/${id}`)
  }

  return {
    contentTypes,
    loading,
    error,
    fetchContentTypes,
    createContentType,
    updateContentType,
    deleteContentType,
  }
}
