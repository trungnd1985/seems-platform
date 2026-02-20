import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import type { User, LoginRequest, LoginResponse } from '@/types/auth'
import { useApi } from '@/composables/useApi'

const TOKEN_KEY = 'seems_admin_token'
const USER_KEY = 'seems_admin_user'

function loadUser(): User | null {
  try {
    const raw = localStorage.getItem(USER_KEY)
    return raw ? (JSON.parse(raw) as User) : null
  } catch {
    return null
  }
}

export const useAuthStore = defineStore('auth', () => {
  const api = useApi()
  const token = ref<string | null>(localStorage.getItem(TOKEN_KEY))
  const user = ref<User | null>(loadUser())

  const isAuthenticated = computed(() => !!token.value)

  async function login(credentials: LoginRequest) {
    const { data } = await api.post<LoginResponse>('/auth/login', credentials)
    token.value = data.accessToken
    user.value = data.user
    localStorage.setItem(TOKEN_KEY, data.accessToken)
    localStorage.setItem(USER_KEY, JSON.stringify(data.user))
  }

  function logout() {
    token.value = null
    user.value = null
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
  }

  return { token, user, isAuthenticated, login, logout }
})
