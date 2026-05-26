import type {
  ArchObject,
  AuthResponse,
  PagedResult,
  ReferenceItem,
  ObjectCategory,
  AuditLogEntry,
} from './types'

const BASE_URL = (import.meta.env.VITE_API_URL as string) || 'http://localhost:5126'

function getToken(): string | null {
  return localStorage.getItem('inpad_token')
}

async function request<T = unknown>(
  path: string,
  init: RequestInit = {},
  isFormData = false,
): Promise<T> {
  const headers: Record<string, string> = {}
  if (!isFormData) headers['Content-Type'] = 'application/json'
  const token = getToken()
  if (token) headers['Authorization'] = `Bearer ${token}`

  const res = await fetch(`${BASE_URL}${path}`, {
    ...init,
    headers: { ...headers, ...(init.headers as Record<string, string> | undefined) },
  })

  if (!res.ok) {
    let message = `${res.status} ${res.statusText}`
    try {
      const data = await res.json()
      if (data?.message) message = data.message
      else if (typeof data === 'string') message = data
    } catch {
      /* ignore */
    }
    if (res.status === 401) {
      localStorage.removeItem('inpad_token')
      localStorage.removeItem('inpad_user')
    }
    throw new Error(message)
  }

  if (res.status === 204) return undefined as T
  const ct = res.headers.get('content-type') || ''
  if (ct.includes('application/json')) return (await res.json()) as T
  return (await res.text()) as unknown as T
}

async function blob(path: string): Promise<Blob> {
  const token = getToken()
  const res = await fetch(`${BASE_URL}${path}`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  })
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`)
  return await res.blob()
}

export const api = {
  baseUrl: BASE_URL,

  // ---- Auth ----
  login: (email: string, password: string) =>
    request<AuthResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    }),
  register: (payload: Record<string, unknown>) =>
    request<AuthResponse | void>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify(payload),
    }),

  // ---- Objects ----
  listObjects: (params: Record<string, string | number | undefined> = {}) => {
    const q = new URLSearchParams()
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== '' && v !== null) q.append(k, String(v))
    })
    const qs = q.toString()
    return request<PagedResult<ArchObject>>(`/api/objects${qs ? `?${qs}` : ''}`)
  },
  getObject: (id: number) => request<ArchObject>(`/api/objects/${id}`),
  createObject: (body: Partial<ArchObject>) =>
    request<ArchObject>('/api/objects', { method: 'POST', body: JSON.stringify(body) }),
  updateObject: (id: number, body: Partial<ArchObject>) =>
    request<ArchObject>(`/api/objects/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  deleteObject: (id: number) => request<void>(`/api/objects/${id}`, { method: 'DELETE' }),
  publishObject: (id: number) =>
    request<ArchObject>(`/api/objects/${id}/publish`, { method: 'POST' }),
  unpublishObject: (id: number) =>
    request<ArchObject>(`/api/objects/${id}/unpublish`, { method: 'POST' }),
  duplicateObject: (id: number) =>
    request<ArchObject>(`/api/objects/${id}/duplicate`, { method: 'POST' }),

  // ---- Export ----
  exportObject: (id: number, format: 'pptx' | 'docx' | 'pdf' | 'txt') =>
    blob(`/api/objects/${id}/export/${format}`),

  // ---- Media ----
  uploadMedia: (form: FormData) =>
    request<unknown>('/api/media/upload', { method: 'POST', body: form }, true),
  updateMedia: (id: number, body: Record<string, unknown>) =>
    request<unknown>(`/api/media/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  reorderMedia: (items: Array<{ id: number; sortOrder: number }>) =>
    request<unknown>('/api/media/reorder', { method: 'POST', body: JSON.stringify(items) }),
  deleteMedia: (id: number) => request<void>(`/api/media/${id}`, { method: 'DELETE' }),

  // ---- References ----
  listReferences: (type: string) => request<ReferenceItem[]>(`/api/references/${type}`),
  createReference: (type: string, body: Partial<ReferenceItem>) =>
    request<ReferenceItem>(`/api/references/${type}`, {
      method: 'POST',
      body: JSON.stringify(body),
    }),
  updateReference: (type: string, id: number, body: Partial<ReferenceItem>) =>
    request<ReferenceItem>(`/api/references/${type}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(body),
    }),
  deleteReference: (type: string, id: number) =>
    request<void>(`/api/references/${type}/${id}`, { method: 'DELETE' }),

  // ---- Categories ----
  listCategories: () => request<ObjectCategory[]>('/api/categories'),

  // ---- Audit ----
  listAudit: (params: Record<string, string | number | undefined> = {}) => {
    const q = new URLSearchParams()
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== '' && v !== null) q.append(k, String(v))
    })
    const qs = q.toString()
    return request<AuditLogEntry[]>(`/api/audit${qs ? `?${qs}` : ''}`)
  },
}