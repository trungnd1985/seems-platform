import { type Component } from 'vue'
import RichText from '~/components/content/RichText.vue'
import HeroBlock from '~/components/content/HeroBlock.vue'
import ImageBlock from '~/components/content/ImageBlock.vue'

const contentTypeMap: Record<string, Component> = {
  'rich-text': RichText,
  'hero-block': HeroBlock,
  'image-block': ImageBlock,
}

export function resolveContentComponent(contentTypeKey: string): Component | null {
  return contentTypeMap[contentTypeKey] ?? null
}

export function registerContentComponent(key: string, component: Component) {
  contentTypeMap[key] = component
}
