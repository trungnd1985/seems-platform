import type { SeoMeta } from '~/types/page'

export function useSeo(seo: SeoMeta) {
  useHead({
    title: seo.title,
    link: seo.canonical ? [{ rel: 'canonical', href: seo.canonical }] : [],
  })

  useServerSeoMeta({
    title: seo.title,
    description: seo.description,
    ogTitle: seo.ogTitle ?? seo.title,
    ogDescription: seo.ogDescription ?? seo.description,
    ogImage: seo.ogImage,
    robots: seo.robots,
  })
}
