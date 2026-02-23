import { Plugin, FileRepository } from 'ckeditor5'
import type { FileLoader, UploadAdapter } from 'ckeditor5'
import { useAuthStore } from '@/stores/auth'

class MediaUploadAdapter implements UploadAdapter {
  private loader: FileLoader
  private xhr: XMLHttpRequest | null = null

  constructor(loader: FileLoader) {
    this.loader = loader
  }

  upload(): Promise<Record<string, string>> {
    return this.loader.file.then(
      (file) =>
        new Promise<Record<string, string>>((resolve, reject) => {
          if (!file) {
            reject(new Error('No file provided'))
            return
          }

          const auth = useAuthStore()
          const formData = new FormData()
          formData.append('file', file)

          this.xhr = new XMLHttpRequest()
          this.xhr.open('POST', '/api/media/upload', true)

          if (auth.token) {
            this.xhr.setRequestHeader('Authorization', `Bearer ${auth.token}`)
          }

          this.xhr.addEventListener('error', () => reject(new Error('Upload failed due to a network error.')))
          this.xhr.addEventListener('abort', () => reject(new Error('Upload aborted.')))

          this.xhr.addEventListener('load', () => {
            const xhr = this.xhr!

            if (xhr.status < 200 || xhr.status >= 300) {
              try {
                const err = JSON.parse(xhr.responseText) as { message?: string }
                reject(new Error(err.message ?? `Upload failed: HTTP ${xhr.status}`))
              } catch {
                reject(new Error(`Upload failed: HTTP ${xhr.status}`))
              }
              return
            }

            try {
              const response = JSON.parse(xhr.responseText) as { url: string }
              resolve({ default: response.url })
            } catch {
              reject(new Error('Invalid response from media server.'))
            }
          })

          if (this.xhr.upload) {
            this.xhr.upload.addEventListener('progress', (evt) => {
              if (evt.lengthComputable) {
                this.loader.uploadTotal = evt.total
                this.loader.uploaded = evt.loaded
              }
            })
          }

          this.xhr.send(formData)
        }),
    )
  }

  abort(): void {
    this.xhr?.abort()
  }
}

export class MediaUploadAdapterPlugin extends Plugin {
  static get pluginName() {
    return 'MediaUploadAdapter' as const
  }

  // Use the class reference, not a string — string lookup is unreliable in CKEditor 5 v42+
  static get requires() {
    return [FileRepository] as const
  }

  afterInit(): void {
    const fileRepository = this.editor.plugins.get(FileRepository)

    // Always overwrite — CloudServices may have cleared createUploadAdapter
    // if no tokenUrl is configured, leaving the slot empty.
    fileRepository.createUploadAdapter = (loader: FileLoader) => new MediaUploadAdapter(loader)

    // Re-evaluate the upload command's enabled state now that an adapter exists.
    this.editor.commands.get('uploadImage')?.refresh()
  }
}
