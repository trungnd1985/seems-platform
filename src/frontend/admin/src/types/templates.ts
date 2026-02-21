export interface TemplateSlotDef {
  key: string
  label: string
  maxItems?: number | null
}

export interface Template {
  id: string
  key: string
  name: string
  themeKey: string
  themeExists: boolean
  slots: TemplateSlotDef[]
  createdAt: string
  updatedAt: string
}

export interface CreateTemplateRequest {
  key: string
  name: string
  themeKey: string
  slots: TemplateSlotDef[]
}

export interface UpdateTemplateRequest {
  name: string
  themeKey: string
  slots: TemplateSlotDef[]
}
