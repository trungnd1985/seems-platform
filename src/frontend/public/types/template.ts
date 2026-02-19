export interface TemplateSlotDef {
  key: string
  label: string
  allowedTypes?: string[]
  maxItems?: number
}

export interface Template {
  id: string
  key: string
  name: string
  themeKey: string
  slots: TemplateSlotDef[]
}
