import type { Page } from '~/types/page'
import { useApi } from '~/utils/api'

export function usePageResolver(slug: string) {
  const api = useApi()

  return useAsyncData(
    `page:${slug}`,
    () => api<Page>(`/pages/by-slug/${encodeURIComponent(slug)}`),
  )
}
