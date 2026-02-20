import { ref } from 'vue'
import { useApi } from '@/composables/useApi'
import type { Theme, CreateThemeRequest, UpdateThemeRequest } from '@/types/themes'

export function useThemes() {
  const api = useApi()
  const themes = ref<Theme[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchThemes() {
    loading.value = true
    error.value = null
    try {
      const { data } = await api.get<Theme[]>('/themes')
      themes.value = data
    } catch (e: any) {
      error.value = e.response?.data?.message ?? 'Failed to load themes.'
    } finally {
      loading.value = false
    }
  }

  async function createTheme(payload: CreateThemeRequest): Promise<Theme> {
    const { data } = await api.post<Theme>('/themes', payload)
    return data
  }

  async function updateTheme(id: string, payload: UpdateThemeRequest): Promise<Theme> {
    const { data } = await api.put<Theme>(`/themes/${id}`, payload)
    return data
  }

  async function deleteTheme(id: string): Promise<void> {
    await api.delete(`/themes/${id}`)
  }

  return {
    themes,
    loading,
    error,
    fetchThemes,
    createTheme,
    updateTheme,
    deleteTheme,
  }
}
