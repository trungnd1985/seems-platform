import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import type { User, LoginRequest, LoginResponse } from '@/types/auth'
import { useApi } from '@/composables/useApi'

const TOKEN_KEY = 'seems_admin_token'

export const useAuthStore = defineStore('auth', () => {
  const api = useApi()
  const token = ref<string | null>(localStorage.getItem(TOKEN_KEY))
  const user = ref<User | null>(null)

  const isAuthenticated = computed(() => !!token.value)

  async function login(credentials: LoginRequest) {
    const { data } = await api.post<LoginResponse>('/auth/login', credentials)
    token.value = data.accessToken
    user.value = data.user
    localStorage.setItem(TOKEN_KEY, data.accessToken)
  }

  function logout() {
    token.value = null
    user.value = null
    localStorage.removeItem(TOKEN_KEY)
  }

  return { token, user, isAuthenticated, login, logout }
})
