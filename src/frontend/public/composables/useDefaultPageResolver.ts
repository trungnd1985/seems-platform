import type { Page } from '~/types/page'
import { useApi } from '~/utils/api'

export function useDefaultPageResolver() {
  const api = useApi()
  const previewToken = useCookie('seems_preview_token')
  const cacheKey = previewToken.value ? 'preview:page:default' : 'page:default'

  return useAsyncData(cacheKey, () => api<Page>('/pages/default'))
}
