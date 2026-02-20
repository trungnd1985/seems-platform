export interface User {
  id: string
  email: string
  displayName: string
  roles: string[]
  isLockedOut: boolean
  lockoutEnd: string | null
  createdAt: string
}

export interface CreateUserRequest {
  email: string
  displayName: string
  password: string
  roles: string[]
}

export interface UpdateUserRequest {
  email: string
  displayName: string
  roles: string[]
}

export interface PaginatedUsers {
  items: User[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}
