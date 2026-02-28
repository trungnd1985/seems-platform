import { ref } from 'vue'
import { useApi } from '@/composables/useApi'
import type { Module, RegisterModuleRequest, UpdateModuleRequest, ModuleStatus } from '@/types/modules'

export function useModules() {
  const api = useApi()
  const modules = ref<Module[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchModules() {
    loading.value = true
    error.value = null
    try {
      const { data } = await api.get<Module[]>('/modules')
      modules.value = data
    } catch (e: any) {
      error.value = e.response?.data?.message ?? 'Failed to load modules.'
    } finally {
      loading.value = false
    }
  }

  async function registerModule(payload: RegisterModuleRequest): Promise<Module> {
    const { data } = await api.post<Module>('/modules', payload)
    return data
  }

  async function updateModule(id: string, payload: UpdateModuleRequest): Promise<Module> {
    const { data } = await api.put<Module>(`/modules/${id}`, payload)
    return data
  }

  async function setStatus(id: string, status: ModuleStatus): Promise<Module> {
    const { data } = await api.patch<Module>(`/modules/${id}`, { status })
    return data
  }

  async function deleteModule(id: string): Promise<void> {
    await api.delete(`/modules/${id}`)
  }

  return { modules, loading, error, fetchModules, registerModule, updateModule, setStatus, deleteModule }
}
