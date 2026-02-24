// Dynamic URL source for @nuxtjs/sitemap (active in production only).
// Uses defineEventHandler directly so this file compiles cleanly in dev
// even when @nuxtjs/sitemap is not loaded.
export default defineEventHandler(async () => {
  const config = useRuntimeConfig()
  const apiBase = config.apiBase as string

  try {
    const pages = await $fetch<{ slug: string; updatedAt: string }[]>(
      `${apiBase}/pages/sitemap`,
    )

    return pages.map((page) => ({
      loc: page.slug === '/' ? '/' : `/${page.slug}`,
      lastmod: page.updatedAt,
    }))
  } catch {
    return []
  }
})
