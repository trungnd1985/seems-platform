export interface ModuleManifest {
  moduleKey: string
  name: string
  version: string
  contentTypes?: { key: string; schema: Record<string, unknown> }[]
  apis?: { method: string; path: string }[]
}

export interface ModuleComponent {
  moduleKey: string
  componentName: string
  loader: () => Promise<unknown>
}
