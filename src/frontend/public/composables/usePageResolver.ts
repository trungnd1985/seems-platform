import type { Page } from '~/types/page'
import { useApi } from '~/utils/api'

export function usePageResolver(slug: string) {
  const api = useApi()
  const previewToken = useCookie('seems_preview_token')
  const cacheKey = previewToken.value ? `preview:page:${slug}` : `page:${slug}`

  return useAsyncData(
    cacheKey,
    () => api<Page>(`/pages/by-slug/${encodeURIComponent(slug)}`),
  )
}
