import { type Component } from 'vue'
import StandardPage from '~/components/templates/StandardPage.vue'
import LandingPage from '~/components/templates/LandingPage.vue'
import BlogPost from '~/components/templates/BlogPost.vue'

// Base template map — used when no theme override exists
const templateMap: Record<string, Component> = {
  'standard-page': StandardPage,
  'landing-page': LandingPage,
  'blog-post': BlogPost,
}

// Theme-specific overrides: themeKey → templateKey → Component
// Add entries here when a theme ships its own template variants
const themeTemplateMap: Record<string, Record<string, Component>> = {
  // example: 'dark-theme': { 'standard-page': DarkStandardPage }
}

export function useTemplateResolver(
  templateKey: string,
  themeKey?: string | null,
): Component {
  if (themeKey) {
    const themeOverride = themeTemplateMap[themeKey]?.[templateKey]
    if (themeOverride) return themeOverride
  }
  return templateMap[templateKey] ?? StandardPage
}
