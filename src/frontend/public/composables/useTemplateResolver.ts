import { type Component } from 'vue'
import StandardPage from '~/components/templates/StandardPage.vue'
import LandingPage from '~/components/templates/LandingPage.vue'
import BlogPost from '~/components/templates/BlogPost.vue'

const templateMap: Record<string, Component> = {
  'standard-page': StandardPage,
  'landing-page': LandingPage,
  'blog-post': BlogPost,
}

export function useTemplateResolver(templateKey: string): Component {
  return templateMap[templateKey] ?? StandardPage
}
