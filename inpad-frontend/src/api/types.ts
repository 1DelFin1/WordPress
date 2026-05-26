export interface ObjectMedia {
  id: number
  url: string
  fileName: string
  title?: string
  mediaType: string
  useOnWebsite: boolean
  useInPresentation: boolean
  useInPortfolio: boolean
  sortOrder: number
}

export interface ObjectCharacteristic {
  id: number
  key: string
  label?: string
  value?: string
  unit?: string
  sortOrder: number
}

export interface ObjectTeamMember {
  id: number
  name: string
  role: string
  sortOrder: number
}

export interface ObjectCategory {
  id: number
  name: string
  slug: string
}

export interface ArchObject {
  id: number
  name: string
  shortName?: string
  city?: string
  address?: string
  objectType?: string
  projectStatus?: string
  designStage?: string
  status: string
  yearStart?: number
  yearEnd?: number
  client?: string
  inpadRole?: string
  shortDescription?: string
  fullDescription?: string
  seoTitle?: string
  seoDescription?: string
  seoKeywords?: string
  slug?: string
  ogImageUrl?: string
  wordPressStatus: string
  wordPressPostId?: number
  publishedAt?: string
  createdAt: string
  updatedAt: string
  createdBy?: string
  media: ObjectMedia[]
  characteristics: ObjectCharacteristic[]
  teamMembers: ObjectTeamMember[]
  categories: ObjectCategory[]
}

export interface User {
  id: number
  email: string
  name: string
  role: string
}

export interface AuthResponse {
  token: string
  user: User
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export interface ReferenceItem {
  id: number
  value: string
  sortOrder?: number
  createdAt?: string
  isActive?: boolean
}

export interface AuditLogEntry {
  id: number
  entityType: string
  entityId?: number
  action: string
  userId?: number
  userName?: string
  createdAt: string
  details?: string
}