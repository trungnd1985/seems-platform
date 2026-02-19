export default defineNuxtRouteMiddleware((to) => {
  const previewToken = to.query.preview as string | undefined

  if (previewToken) {
    const previewCookie = useCookie('seems_preview_token', { maxAge: 3600 })
    previewCookie.value = previewToken
  }
})
