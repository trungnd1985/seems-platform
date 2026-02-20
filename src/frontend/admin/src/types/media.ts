export interface MediaItem {
  id: string
  fileName: string
  originalName: string
  url: string
  mimeType: string
  size: number
  altText: string | null
  caption: string | null
  ownerId: string
  ownerEmail: string | null
  folderId: string | null
  createdAt: string
}

export interface MediaFolder {
  id: string
  name: string
  ownerId: string
  parentId: string | null
  childCount: number
  mediaCount: number
  createdAt: string
}

export interface MediaPage {
  items: MediaItem[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

export interface LocalStorageConfig {
  baseUrl: string
}

export interface S3StorageConfig {
  bucketName: string
  region: string
  serviceUrl: string
  accessKey: string
  secretKey: string
}

export interface StorageSettings {
  provider: 'local' | 's3'
  local: LocalStorageConfig
  s3: S3StorageConfig
}
