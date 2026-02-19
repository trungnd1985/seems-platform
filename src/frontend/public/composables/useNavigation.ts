import { useApi } from '~/utils/api'

export interface NavItem {
  label: string
  slug: string
  children?: NavItem[]
}

export function useNavigation() {
  const api = useApi()

  return useAsyncData(
    'navigation',
    () => api<NavItem[]>('/navigation'),
    { default: () => [] },
  )
}
