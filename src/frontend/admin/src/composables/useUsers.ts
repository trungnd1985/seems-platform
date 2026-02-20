import { ref } from 'vue'
import { useApi } from '@/composables/useApi'
import type { User, CreateUserRequest, UpdateUserRequest, PaginatedUsers } from '@/types/users'

export function useUsers() {
  const api = useApi()
  const users = ref<User[]>([])
  const total = ref(0)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchUsers(page = 1, pageSize = 20) {
    loading.value = true
    error.value = null
    try {
      const { data } = await api.get<PaginatedUsers>('/users', { params: { page, pageSize } })
      users.value = data.items
      total.value = data.total
    } catch (e: any) {
      error.value = e.response?.data?.message ?? 'Failed to load users.'
    } finally {
      loading.value = false
    }
  }

  async function createUser(payload: CreateUserRequest): Promise<User> {
    const { data } = await api.post<User>('/users', payload)
    return data
  }

  async function updateUser(id: string, payload: UpdateUserRequest): Promise<User> {
    const { data } = await api.put<User>(`/users/${id}`, payload)
    return data
  }

  async function deleteUser(id: string): Promise<void> {
    await api.delete(`/users/${id}`)
  }

  async function resetPassword(id: string, newPassword: string): Promise<void> {
    await api.post(`/users/${id}/reset-password`, { newPassword })
  }

  async function lockUser(id: string): Promise<void> {
    await api.post(`/users/${id}/lock`)
  }

  async function unlockUser(id: string): Promise<void> {
    await api.post(`/users/${id}/unlock`)
  }

  return {
    users,
    total,
    loading,
    error,
    fetchUsers,
    createUser,
    updateUser,
    deleteUser,
    resetPassword,
    lockUser,
    unlockUser,
  }
}
