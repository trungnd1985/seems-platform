import { defineAsyncComponent, type Component } from 'vue'
import { getModuleComponent } from '~/utils/module-registry'
import ModuleFallback from '~/components/modules/ModuleFallback.vue'

export function useModuleLoader(moduleKey: string): Component {
  const loader = getModuleComponent(moduleKey)

  if (!loader) {
    return ModuleFallback
  }

  return defineAsyncComponent({
    loader,
    loadingComponent: ModuleFallback,
    errorComponent: ModuleFallback,
    timeout: 10000,
  })
}
