export interface Module {
  id: string
  moduleKey: string
  name: string
  version: string
  status: ModuleStatus
  publicComponentUrl?: string
  description?: string
  author?: string
  createdAt: string
  updatedAt: string
}

export type ModuleStatus = 'Installed' | 'Disabled'

export interface ContentTypeDecl {
  key: string
  name: string
  schema: string // serialised JSON schema
}

export interface RegisterModuleRequest {
  moduleKey: string
  name: string
  version: string
  publicComponentUrl?: string
  description?: string
  author?: string
  contentTypes?: ContentTypeDecl[]
}

export interface UpdateModuleRequest {
  name: string
  version: string
  publicComponentUrl?: string
  description?: string
  author?: string
}

export const MODULE_STATUS_SEVERITY: Record<ModuleStatus, 'success' | 'secondary'> = {
  Installed: 'success',
  Disabled: 'secondary',
}
