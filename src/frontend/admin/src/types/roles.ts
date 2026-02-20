export interface Role {
  id: string
  name: string
  description: string | null
  userCount: number
  isSystem: boolean
  createdAt: string
}

export interface CreateRoleRequest {
  name: string
  description: string | null
}

export interface UpdateRoleRequest {
  name: string
  description: string | null
}
