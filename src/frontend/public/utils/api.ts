export function useApi() {
  const config = useRuntimeConfig()
  const previewToken = useCookie('seems_preview_token')

  return $fetch.create({
    baseURL: config.public.apiBase as string,
    headers: {
      'Content-Type': 'application/json',
      ...(previewToken.value ? { 'X-Preview-Token': previewToken.value } : {}),
    },
  })
}
