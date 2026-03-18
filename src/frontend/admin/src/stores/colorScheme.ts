import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export type ColorPreference = 'light' | 'dark' | 'system'

const STORAGE_KEY = 'seems_admin_color_scheme'

export const useColorSchemeStore = defineStore('colorScheme', () => {
  const preference = ref<ColorPreference>(
    (localStorage.getItem(STORAGE_KEY) as ColorPreference) ?? 'system',
  )

  const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')

  const resolvedIsDark = computed(() => {
    if (preference.value === 'dark') return true
    if (preference.value === 'light') return false
    return mediaQuery.matches
  })

  function applyToDocument() {
    document.documentElement.classList.toggle('dark', resolvedIsDark.value)
  }

  function setPreference(pref: ColorPreference) {
    preference.value = pref
    localStorage.setItem(STORAGE_KEY, pref)
    applyToDocument()
  }

  mediaQuery.addEventListener('change', () => {
    if (preference.value === 'system') applyToDocument()
  })

  return { preference, resolvedIsDark, setPreference, applyToDocument }
})
