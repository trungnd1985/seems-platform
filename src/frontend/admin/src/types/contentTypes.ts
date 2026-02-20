export type FieldType =
  | 'text'
  | 'textarea'
  | 'richtext'
  | 'number'
  | 'boolean'
  | 'datetime'
  | 'date'
  | 'select'
  | 'media'
  | 'relation'

export interface ContentField {
  name: string
  label: string
  type: FieldType
  required: boolean
  description?: string
  // type-specific options
  multiple?: boolean       // select: allow multiple values
  options?: string[]       // select: allowed values
  min?: number             // number: minimum
  max?: number             // number: maximum
  maxLength?: number       // text/textarea: character limit
  targetContentTypeKey?: string  // relation: target content type
}

export interface ContentSchema {
  fields: ContentField[]
}

export interface ContentType {
  id: string
  key: string
  name: string
  schema: string  // JSON-encoded ContentSchema
  createdAt: string
  updatedAt: string
}

export interface CreateContentTypeRequest {
  key: string
  name: string
  schema: string
}

export interface UpdateContentTypeRequest {
  name: string
  schema: string
}

export const FIELD_TYPES: { value: FieldType; label: string }[] = [
  { value: 'text', label: 'Text' },
  { value: 'textarea', label: 'Long Text' },
  { value: 'richtext', label: 'Rich Text' },
  { value: 'number', label: 'Number' },
  { value: 'boolean', label: 'Boolean' },
  { value: 'datetime', label: 'Date & Time' },
  { value: 'date', label: 'Date' },
  { value: 'select', label: 'Select' },
  { value: 'media', label: 'Media' },
  { value: 'relation', label: 'Relation' },
]

export const FIELD_TYPE_LABELS: Record<FieldType, string> = {
  text: 'Text',
  textarea: 'Long Text',
  richtext: 'Rich Text',
  number: 'Number',
  boolean: 'Boolean',
  datetime: 'Date & Time',
  date: 'Date',
  select: 'Select',
  media: 'Media',
  relation: 'Relation',
}

export const FIELD_TYPE_ICONS: Record<FieldType, string> = {
  text: 'pi pi-align-left',
  textarea: 'pi pi-align-justify',
  richtext: 'pi pi-file-edit',
  number: 'pi pi-hashtag',
  boolean: 'pi pi-check-square',
  datetime: 'pi pi-calendar-clock',
  date: 'pi pi-calendar',
  select: 'pi pi-list',
  media: 'pi pi-image',
  relation: 'pi pi-link',
}
