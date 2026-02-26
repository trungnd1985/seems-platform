import { useApi } from './useApi'
import type { MediaItem, MediaFolder, MediaPage, StorageSettings, SiteInfo } from '@/types/media'

export function useMedia() {
  const api = useApi()

  async function listMedia(folderId?: string | null, page = 1, pageSize = 40): Promise<MediaPage> {
    const params: Record<string, string | number> = { page, pageSize }
    if (folderId) params.folderId = folderId
    const res = await api.get<MediaPage>('/media', { params })
    return res.data
  }

  async function getMedia(id: string): Promise<MediaItem> {
    const res = await api.get<MediaItem>(`/media/${id}`)
    return res.data
  }

  async function uploadMedia(
    file: File,
    folderId?: string | null,
    onProgress?: (pct: number) => void,
  ): Promise<MediaItem> {
    const form = new FormData()
    form.append('file', file)
    const params: Record<string, string> = {}
    if (folderId) params.folderId = folderId

    const res = await api.post<MediaItem>('/media/upload', form, {
      params,
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: (e) => {
        if (onProgress && e.total) onProgress(Math.round((e.loaded * 100) / e.total))
      },
    })
    return res.data
  }

  async function deleteMedia(id: string): Promise<void> {
    await api.delete(`/media/${id}`)
  }

  async function moveMedia(id: string, targetFolderId: string | null): Promise<MediaItem> {
    const res = await api.patch<MediaItem>(`/media/${id}/move`, { targetFolderId })
    return res.data
  }

  // Folders
  async function listFolders(): Promise<MediaFolder[]> {
    const res = await api.get<MediaFolder[]>('/media/folders')
    return res.data
  }

  async function createFolder(name: string, parentId?: string | null): Promise<MediaFolder> {
    const res = await api.post<MediaFolder>('/media/folders', { name, parentId: parentId ?? null })
    return res.data
  }

  async function renameFolder(id: string, name: string): Promise<MediaFolder> {
    const res = await api.put<MediaFolder>(`/media/folders/${id}`, { name })
    return res.data
  }

  async function deleteFolder(id: string): Promise<void> {
    await api.delete(`/media/folders/${id}`)
  }

  // Settings
  async function getStorageSettings(): Promise<StorageSettings> {
    const res = await api.get<StorageSettings>('/settings/storage')
    return res.data
  }

  async function updateStorageSettings(dto: StorageSettings): Promise<void> {
    await api.put('/settings/storage', dto)
  }

  async function getSiteInfo(): Promise<SiteInfo> {
    const res = await api.get<SiteInfo>('/settings/site')
    return res.data
  }

  async function updateSiteInfo(dto: SiteInfo): Promise<void> {
    await api.put('/settings/site', dto)
  }

  return {
    listMedia,
    getMedia,
    uploadMedia,
    deleteMedia,
    moveMedia,
    listFolders,
    createFolder,
    renameFolder,
    deleteFolder,
    getStorageSettings,
    updateStorageSettings,
    getSiteInfo,
    updateSiteInfo,
  }
}
