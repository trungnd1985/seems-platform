import type { ContentItem } from '~/types/content'
import { useApi } from '~/utils/api'

export function useContentItem(id: string) {
  const api = useApi()

  return useAsyncData(
    `content:${id}`,
    () => api<ContentItem>(`/content-items/${id}`),
  )
}
