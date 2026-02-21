import { ref } from 'vue'
import { useApi } from '@/composables/useApi'
import type { Template, CreateTemplateRequest, UpdateTemplateRequest } from '@/types/templates'

export function useTemplates() {
  const api = useApi()
  const templates = ref<Template[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchTemplates() {
    loading.value = true
    error.value = null
    try {
      const { data } = await api.get<Template[]>('/templates')
      templates.value = data
    } catch (e: any) {
      error.value = e.response?.data?.message ?? 'Failed to load templates.'
    } finally {
      loading.value = false
    }
  }

  async function createTemplate(payload: CreateTemplateRequest): Promise<Template> {
    const { data } = await api.post<Template>('/templates', payload)
    return data
  }

  async function updateTemplate(id: string, payload: UpdateTemplateRequest): Promise<Template> {
    const { data } = await api.put<Template>(`/templates/${id}`, payload)
    return data
  }

  async function deleteTemplate(id: string): Promise<void> {
    await api.delete(`/templates/${id}`)
  }

  return {
    templates,
    loading,
    error,
    fetchTemplates,
    createTemplate,
    updateTemplate,
    deleteTemplate,
  }
}
