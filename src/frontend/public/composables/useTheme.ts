import { computed } from 'vue'
import type { Theme } from '~/types/theme'
import { useApi } from '~/utils/api'

export function useTheme(themeKey: string | undefined | null) {
  if (!themeKey) return

  const api = useApi()
  const { data: theme } = useAsyncData(
    `theme:${themeKey}`,
    () => api<Theme>(`/themes/by-key/${encodeURIComponent(themeKey)}`),
  )

  useHead(computed(() => ({
    htmlAttrs: { 'data-theme': themeKey },
    link: theme.value?.cssUrl
      ? [{ rel: 'stylesheet', href: theme.value.cssUrl, key: `theme-css-${themeKey}` }]
      : [],
  })))
}
