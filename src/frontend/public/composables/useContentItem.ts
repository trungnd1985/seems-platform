import type { ContentItem } from '~/types/content'
import { useApi } from '~/utils/api'

export function useContentItem(id: string) {
  const api = useApi()
  const previewToken = useCookie('seems_preview_token')
  const cacheKey = previewToken.value ? `preview:content:${id}` : `content:${id}`

  return useAsyncData(
    cacheKey,
    () => api<ContentItem>(`/content-items/${id}`),
  )
}
