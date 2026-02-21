export interface Theme {
  id: string
  key: string
  name: string
  description: string | null
  cssUrl: string | null
  createdAt: string
  updatedAt: string
}

export interface CreateThemeRequest {
  key: string
  name: string
  description: string | null
  cssUrl: string | null
}

export interface UpdateThemeRequest {
  name: string
  description: string | null
  cssUrl: string | null
}
