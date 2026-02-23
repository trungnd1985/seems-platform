<script setup lang="ts">
import { ref, computed } from 'vue'
import { Ckeditor } from '@ckeditor/ckeditor5-vue'
import {
  ClassicEditor,
  Autosave,
  Essentials,
  Paragraph,
  Autoformat,
  TextTransformation,
  LinkImage,
  Link,
  ImageBlock,
  ImageToolbar,
  BlockQuote,
  Bold,
  CloudServices,
  ImageUpload,
  ImageInsertViaUrl,
  AutoImage,
  Table,
  TableToolbar,
  Emoji,
  Mention,
  Heading,
  ImageTextAlternative,
  ImageCaption,
  ImageStyle,
  Indent,
  IndentBlock,
  ImageInline,
  Italic,
  List,
  MediaEmbed,
  TableCaption,
  TodoList,
  Underline,
  PlainTableOutput,
  Fullscreen,
  Markdown,
  PasteFromMarkdownExperimental,
  ShowBlocks,
  SourceEditing,
  GeneralHtmlSupport,
  Title,
} from 'ckeditor5'

import 'ckeditor5/ckeditor5.css'
import '@/assets/css/ckeditor.css'
import { MediaUploadAdapterPlugin } from '@/plugins/ckeditorUploadAdapter'
import { InsertFromMediaLibraryPlugin } from '@/plugins/ckeditorMediaLibrary'
import MediaPickerDialog from '@/components/MediaPickerDialog.vue'
import type { MediaItem } from '@/types/media'

const props = defineProps<{
  modelValue: string
  invalid?: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

// ── Media picker bridge ───────────────────────────────────────────────────────
// When the CKEditor toolbar button fires, it passes us an insertFn.
// We store it, open the dialog, and call it once the user picks a file.
const mediaPickerVisible = ref(false)
const pendingInsertFn = ref<((src: string, alt: string) => void) | null>(null)

function openMediaPicker(insertFn: (src: string, alt: string) => void) {
  pendingInsertFn.value = insertFn
  mediaPickerVisible.value = true
}

function onMediaSelected(item: MediaItem) {
  pendingInsertFn.value?.(item.url, item.altText ?? item.originalName)
  pendingInsertFn.value = null
  mediaPickerVisible.value = false
}

function onPickerClosed() {
  pendingInsertFn.value = null
}
// ─────────────────────────────────────────────────────────────────────────────

const editorConfig = computed(() => ({
  licenseKey: 'GPL',
  plugins: [
    Autoformat,
    AutoImage,
    Autosave,
    BlockQuote,
    Bold,
    CloudServices,
    Emoji,
    Essentials,
    Fullscreen,
    GeneralHtmlSupport,
    Heading,
    ImageBlock,
    ImageCaption,
    ImageInline,
    ImageInsertViaUrl,
    ImageStyle,
    ImageTextAlternative,
    ImageToolbar,
    ImageUpload,
    MediaUploadAdapterPlugin,
    InsertFromMediaLibraryPlugin,
    Indent,
    IndentBlock,
    Italic,
    Link,
    LinkImage,
    List,
    Markdown,
    MediaEmbed,
    Mention,
    Paragraph,
    PasteFromMarkdownExperimental,
    PlainTableOutput,
    ShowBlocks,
    SourceEditing,
    Table,
    TableCaption,
    TableToolbar,
    TextTransformation,
    Title,
    TodoList,
    Underline,
  ],
  // Pass the Vue-side callback into the plugin via config
  mediaLibrary: {
    onInsert: openMediaPicker,
  },
  toolbar: {
    items: [
      'undo',
      'redo',
      '|',
      'sourceEditing',
      'showBlocks',
      'fullscreen',
      '|',
      'heading',
      '|',
      'bold',
      'italic',
      'underline',
      '|',
      'emoji',
      'link',
      'insertFromMediaLibrary',
      'insertImageViaUrl',
      'mediaEmbed',
      'insertTable',
      'blockQuote',
      '|',
      'bulletedList',
      'numberedList',
      'todoList',
      'outdent',
      'indent',
    ],
    shouldNotGroupWhenFull: false,
  },
  fullscreen: {
    onEnterCallback: (container: HTMLElement) =>
      container.classList.add(
        'editor-container',
        'editor-container_classic-editor',
        'editor-container_include-fullscreen',
        'main-container',
      ),
  },
  heading: {
    options: [
      { model: 'paragraph' as const, title: 'Paragraph', class: 'ck-heading_paragraph' },
      { model: 'heading1' as const, view: 'h1' as const, title: 'Heading 1', class: 'ck-heading_heading1' },
      { model: 'heading2' as const, view: 'h2' as const, title: 'Heading 2', class: 'ck-heading_heading2' },
      { model: 'heading3' as const, view: 'h3' as const, title: 'Heading 3', class: 'ck-heading_heading3' },
      { model: 'heading4' as const, view: 'h4' as const, title: 'Heading 4', class: 'ck-heading_heading4' },
      { model: 'heading5' as const, view: 'h5' as const, title: 'Heading 5', class: 'ck-heading_heading5' },
      { model: 'heading6' as const, view: 'h6' as const, title: 'Heading 6', class: 'ck-heading_heading6' },
    ],
  },
  list: {
    properties: {
      styles: true,
      startIndex: true,
      reversed: true,
    },
  },
  htmlSupport: {
    allow: [
      {
        name: /^.*$/,
        styles: true as const,
        attributes: true as const,
        classes: true as const,
      },
    ],
  },
  image: {
    toolbar: [
      'toggleImageCaption',
      'imageTextAlternative',
      '|',
      'imageStyle:inline',
      'imageStyle:wrapText',
      'imageStyle:breakText',
    ],
  },
  link: {
    addTargetToExternalLinks: true,
    defaultProtocol: 'https://',
    decorators: {
      toggleDownloadable: {
        mode: 'manual' as const,
        label: 'Downloadable',
        attributes: { download: 'file' },
      },
    },
  },
  mention: {
    feeds: [{ marker: '@', feed: [] }],
  },
  menuBar: {
    isVisible: true,
  },
  placeholder: 'Type or paste your content here!',
  table: {
    contentToolbar: ['tableColumn', 'tableRow', 'mergeTableCells'],
  },
}))
</script>

<template>
  <div class="editor-container editor-container_classic-editor editor-container_include-fullscreen">
    <div class="editor-container__editor">
      <div class="rte-wrap" :class="{ 'rte-wrap--invalid': invalid }">
        <Ckeditor
          :editor="ClassicEditor"
          :model-value="modelValue"
          @update:model-value="emit('update:modelValue', $event as string)"
          :config="editorConfig"
        />
      </div>
    </div>
  </div>

  <MediaPickerDialog
    :visible="mediaPickerVisible"
    @update:visible="(v) => { mediaPickerVisible = v; if (!v) onPickerClosed() }"
    @selected="onMediaSelected"
  />
</template>

<style scoped>
.rte-wrap {
  border-radius: var(--p-inputtext-border-radius, 6px);
  overflow: hidden;
}

.rte-wrap--invalid :deep(.ck.ck-editor__editable:not(.ck-editor__nested-editable)) {
  border-color: var(--p-red-500) !important;
}

.rte-wrap :deep(.ck-editor__editable_inline) {
  min-height: 180px;
  max-height: 420px;
}

.rte-wrap :deep(.ck.ck-toolbar) {
  border-color: var(--p-content-border-color);
  background: var(--p-content-hover-background);
}

.rte-wrap :deep(.ck.ck-editor__main > .ck-editor__editable) {
  background: var(--p-inputtext-background);
  color: var(--p-text-color);
  border-color: var(--p-content-border-color);
}

.rte-wrap :deep(.ck.ck-editor__editable.ck-focused:not(.ck-editor__nested-editable)) {
  border-color: var(--p-primary-color);
  box-shadow: 0 0 0 1px var(--p-primary-color);
}
</style>
