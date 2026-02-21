import type { InstalledModule } from '~/types/module'
import { registerModuleComponent } from '~/utils/module-registry'

export default defineNuxtPlugin(async () => {
  const config = useRuntimeConfig()
  const apiBase = config.public.apiBase as string

  let modules: InstalledModule[] = []

  try {
    modules = await $fetch<InstalledModule[]>(`${apiBase}/modules/installed`)
  }
  catch {
    // Non-fatal: if the registry can't be reached, modules simply won't render.
    console.warn('[SEEMS] Could not load installed modules from API.')
    return
  }

  for (const mod of modules) {
    if (!mod.publicComponentUrl) continue

    const url = mod.publicComponentUrl

    registerModuleComponent(mod.moduleKey, async () => {
      // `@vite-ignore` suppresses Vite's static analysis warning for runtime URLs.
      const remote = await import(/* @vite-ignore */ url)
      // Module JS must export the Vue component as `default`.
      return remote.default ?? remote
    })
  }
})
