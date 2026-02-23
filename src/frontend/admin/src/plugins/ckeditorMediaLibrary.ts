import { Plugin, ButtonView } from 'ckeditor5'

// Landscape/photo icon matching CKEditor's 20×20 SVG style
const mediaLibraryIcon = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20">
  <path d="M17 3H3a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h14a1 1 0 0 0 1-1V4a1 1 0 0 0-1-1zm-1 12H4V5h12v10zm-8-3 2-2.7 1.45 1.93L13 9l3 6H4z"/>
</svg>`

// Callback type: plugin calls onInsert(insertFn) when the button is clicked.
// The Vue component handles the dialog and calls insertFn(src, alt) on confirm.
export type OnInsertMedia = (insertFn: (src: string, alt: string) => void) => void

export class InsertFromMediaLibraryPlugin extends Plugin {
  static get pluginName() {
    return 'InsertFromMediaLibrary' as const
  }

  init(): void {
    const editor = this.editor
    const onInsert = editor.config.get('mediaLibrary.onInsert') as OnInsertMedia | undefined

    if (!onInsert) return

    editor.ui.componentFactory.add('insertFromMediaLibrary', (locale) => {
      const button = new ButtonView(locale)

      button.set({
        label: 'Media Library',
        icon: mediaLibraryIcon,
        tooltip: true,
      })

      button.on('execute', () => {
        onInsert((src, alt) => {
          editor.model.change((writer) => {
            const image = writer.createElement('imageBlock', {
              src,
              ...(alt ? { alt } : {}),
            })
            editor.model.insertContent(image, editor.model.document.selection.getFirstPosition()!)
          })
        })
      })

      return button
    })
  }
}
