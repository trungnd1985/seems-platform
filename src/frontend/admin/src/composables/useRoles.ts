import { ref } from 'vue'
import { useApi } from '@/composables/useApi'
import type { Role, CreateRoleRequest, UpdateRoleRequest } from '@/types/roles'

export function useRoles() {
  const api = useApi()
  const roles = ref<Role[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchRoles() {
    loading.value = true
    error.value = null
    try {
      const { data } = await api.get<Role[]>('/roles')
      roles.value = data
    } catch (e: any) {
      error.value = e.response?.data?.message ?? 'Failed to load roles.'
    } finally {
      loading.value = false
    }
  }

  async function createRole(payload: CreateRoleRequest): Promise<Role> {
    const { data } = await api.post<Role>('/roles', payload)
    return data
  }

  async function updateRole(id: string, payload: UpdateRoleRequest): Promise<Role> {
    const { data } = await api.put<Role>(`/roles/${id}`, payload)
    return data
  }

  async function deleteRole(id: string): Promise<void> {
    await api.delete(`/roles/${id}`)
  }

  return { roles, loading, error, fetchRoles, createRole, updateRole, deleteRole }
}
