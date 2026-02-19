type AsyncComponentLoader = () => Promise<unknown>

const moduleComponents: Record<string, AsyncComponentLoader> = {}

export function registerModuleComponent(moduleKey: string, loader: AsyncComponentLoader) {
  moduleComponents[moduleKey] = loader
}

export function getModuleComponent(moduleKey: string): AsyncComponentLoader | null {
  return moduleComponents[moduleKey] ?? null
}

export function getRegisteredModules(): string[] {
  return Object.keys(moduleComponents)
}
