<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import {
  NButton, NInput, NSelect, NModal, NForm, NFormItem, NTag,
  useMessage, useDialog, NPagination
} from 'naive-ui'

interface UserRow {
  id: number
  email: string
  name: string
  role: string
  isActive: boolean
  createdAt: string
  lastLoginAt?: string
}

const base = (import.meta.env.VITE_API_URL as string) || 'http://localhost:5126'
function authHeaders(): Record<string, string> {
  const t = localStorage.getItem('inpad_token')
  return t ? { Authorization: `Bearer ${t}`, 'Content-Type': 'application/json' } : { 'Content-Type': 'application/json' }
}

const message = useMessage()
const dialog = useDialog()

const users = ref<UserRow[]>([])
const loading = ref(false)
const search = ref('')
const filterRole = ref('')
const filterStatus = ref('')
const page = ref(1)
const pageSize = 10

const showAddModal = ref(false)
const addForm = ref({ lastName: '', firstName: '', middleName: '', email: '', password: '', role: 'Editor' })
const addLoading = ref(false)

async function load() {
  loading.value = true
  try {
    const r = await fetch(`${base}/api/users`, { headers: authHeaders() })
    if (r.ok) users.value = await r.json()
    else if (r.status === 403) message.warning('Доступно только администратору')
  } catch (e) { message.error((e as Error).message) }
  finally { loading.value = false }
}

onMounted(load)

const filtered = computed(() => {
  let list = users.value
  if (search.value) {
    const q = search.value.toLowerCase()
    list = list.filter(u => u.name?.toLowerCase().includes(q) || u.email.toLowerCase().includes(q))
  }
  if (filterRole.value) list = list.filter(u => u.role === filterRole.value)
  if (filterStatus.value) list = list.filter(u => filterStatus.value === 'active' ? u.isActive : !u.isActive)

  return list
})

const paginated = computed(() => filtered.value.slice((page.value - 1) * pageSize, page.value * pageSize))

function fmtDate(s?: string) { return s ? new Date(s).toLocaleDateString('ru-RU') : '—' }

function initials(name: string) {
  const parts = name.split(' ').filter(Boolean)
  return parts.slice(0, 2).map(p => p[0]?.toUpperCase()).join('')
}

function roleLabel(r: string) {
  const m: Record<string, string> = { Administrator: 'Администратор', Editor: 'Редактор', Manager: 'Менеджер', Viewer: 'Просмотрщик' }
  return m[r] || r
}

function roleColor(r: string): 'default' | 'success' | 'warning' | 'error' | 'info' {
  const m: Record<string, 'default' | 'success' | 'warning' | 'error' | 'info'> = {
    Administrator: 'error', Editor: 'info', Manager: 'warning', Viewer: 'default'
  }
  return m[r] || 'default'
}

function avatarColor(name: string) {
  const colors = ['#8C8B72', '#6B8E9F', '#9E7B65', '#7B9E65', '#9E6B7B']
  let h = 0
  for (let i = 0; i < name.length; i++) h = (h * 31 + name.charCodeAt(i)) % colors.length
  return colors[h]
}

async function toggleActive(u: UserRow) {
  try {
    const r = await fetch(`${base}/api/users/${u.id}`, {
      method: 'PUT',
      headers: authHeaders(),
      body: JSON.stringify({ isActive: !u.isActive })
    })
    if (r.ok) { u.isActive = !u.isActive; message.success('Статус обновлён') }
    else message.error('Ошибка')
  } catch (e) { message.error((e as Error).message) }
}

function confirmDelete(u: UserRow) {
  dialog.warning({
    title: 'Удалить пользователя?',
    content: `Вы уверены, что хотите удалить пользователя «${u.name}»? Это действие необратимо.`,
    positiveText: 'Удалить',
    negativeText: 'Отмена',
    onPositiveClick: async () => {
      const r = await fetch(`${base}/api/users/${u.id}`, { method: 'DELETE', headers: authHeaders() })
      if (r.ok) { users.value = users.value.filter(x => x.id !== u.id); message.success('Удалён') }
      else message.error('Ошибка удаления')
    }
  })
}

async function submitAdd() {
  addLoading.value = true
  try {
    if (!addForm.value.password) {
      message.error('Введите пароль')
      addLoading.value = false
      return
    }
    const r = await fetch(`${base}/api/users`, {
      method: 'POST',
      headers: authHeaders(),
      body: JSON.stringify({
        name: [addForm.value.lastName, addForm.value.firstName, addForm.value.middleName].filter(Boolean).join(' '),
        email: addForm.value.email,
        password: addForm.value.password,
        role: addForm.value.role
      })
    })
    if (r.ok) {
      message.success('Пользователь создан')
      showAddModal.value = false
      addForm.value = { lastName: '', firstName: '', middleName: '', email: '', password: '', role: 'Editor' }
      await load()
    } else {
      const err = await r.json().catch(() => ({}))
      message.error(err.message || 'Ошибка создания')
    }
  } catch (e) { message.error((e as Error).message) }
  finally { addLoading.value = false }
}

const roleOptions = [
  { label: 'Редактор', value: 'Editor' },
  { label: 'Менеджер', value: 'Manager' },
  { label: 'Просмотрщик', value: 'Viewer' },
  { label: 'Администратор', value: 'Administrator' },
]

const filterRoleOptions = [{ label: 'Все роли', value: '' }, ...roleOptions.map(o => ({ ...o, value: o.value }))]
const filterStatusOptions = [
  { label: 'Все статусы', value: '' },
  { label: 'Активен', value: 'active' },
  { label: 'Заблокирован', value: 'blocked' },
]
</script>

<template>
  <div class="page">
    <div class="top-row">
      <div>
        <h1 class="h1">Пользователи</h1>
        <p class="subtitle">Управление доступом и ролями</p>
      </div>
      <NButton type="primary" @click="showAddModal = true">+ Добавить пользователя</NButton>
    </div>

    <div class="filters">
      <NInput v-model:value="search" placeholder="Поиск по имени или логину..." clearable class="search-input">
        <template #prefix><span style="color:#aaa">🔍</span></template>
      </NInput>
      <NSelect v-model:value="filterRole" :options="filterRoleOptions" style="width:160px" />
      <NSelect v-model:value="filterStatus" :options="filterStatusOptions" style="width:160px" />
    </div>

    <div class="card table-card">
      <table class="tbl">
        <thead>
          <tr>
            <th>Пользователь</th>
            <th>Email</th>
            <th>Роль</th>
            <th>Последний вход</th>
            <th>Дата регистрации</th>
            <th>Статус</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading"><td colspan="7" class="empty">Загрузка...</td></tr>
          <tr v-else-if="paginated.length === 0"><td colspan="7" class="empty">Нет данных</td></tr>
          <tr v-for="u in paginated" :key="u.id">
            <td>
              <div class="user-cell">
                <div class="avatar" :style="{ background: avatarColor(u.name || u.email) }">
                  {{ initials(u.name || u.email) }}
                </div>
                <div>
                  <div class="name">{{ u.name }}</div>
                  <div class="sub">@{{ u.email.split('@')[0] }}</div>
                </div>
              </div>
            </td>
            <td class="email">{{ u.email }}</td>
            <td>
              <NTag size="small" :type="roleColor(u.role)" :bordered="false">{{ roleLabel(u.role) }}</NTag>
            </td>
            <td class="secondary">{{ fmtDate(u.lastLoginAt) }}</td>
            <td class="secondary">{{ fmtDate(u.createdAt) }}</td>
            <td>
              <NTag size="small" :type="u.isActive ? 'success' : 'default'" :bordered="false">
                {{ u.isActive ? 'Активен' : 'Заблокирован' }}
              </NTag>
            </td>
            <td>
              <div class="actions">
                <NButton size="small" @click="toggleActive(u)">{{ u.isActive ? 'Заблокировать' : 'Активировать' }}</NButton>
                <NButton size="small" quaternary @click="confirmDelete(u)">—</NButton>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
      <div class="pagination-row">
        <span class="count">Показано {{ paginated.length }} из {{ filtered.length }} пользователей</span>
        <NPagination
          v-model:page="page"
          :page-count="Math.ceil(filtered.length / pageSize)"
          :page-slot="5"
          simple
        />
      </div>
    </div>

    <NModal v-model:show="showAddModal" :mask-closable="false" style="width:480px">
      <div class="modal-box">
        <div class="modal-title">Добавить пользователя</div>
        <NForm label-placement="top" :show-require-mark="false">
          <div class="grid-2">
            <NFormItem label="Фамилия *">
              <NInput v-model:value="addForm.lastName" placeholder="" />
            </NFormItem>
            <NFormItem label="Имя *">
              <NInput v-model:value="addForm.firstName" placeholder="" />
            </NFormItem>
          </div>
          <NFormItem label="Отчество">
            <NInput v-model:value="addForm.middleName" placeholder="" />
          </NFormItem>
          <div class="grid-2">
            <NFormItem label="Email *">
              <NInput v-model:value="addForm.email" placeholder="" />
            </NFormItem>
            <NFormItem label="Роль *">
              <NSelect v-model:value="addForm.role" :options="roleOptions" />
            </NFormItem>
          </div>
          <NFormItem label="Пароль *">
            <NInput v-model:value="addForm.password" type="password" show-password-on="click" placeholder="Минимум 6 символов" />
          </NFormItem>
        </NForm>
        <div class="modal-actions">
          <NButton @click="showAddModal = false">Отмена</NButton>
          <NButton type="primary" :loading="addLoading" @click="submitAdd">
            Создать и отправить приглашение
          </NButton>
        </div>
      </div>
    </NModal>
  </div>
</template>

<style scoped>
.page { max-width: 1200px; margin: 0 auto; }
.top-row { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 20px; }
.h1 { font-size: 22px; font-weight: 700; margin: 0 0 4px; }
.subtitle { color: var(--color-text-secondary); font-size: 13px; margin: 0; }
.filters { display: flex; gap: 12px; margin-bottom: 16px; }
.search-input { flex: 1; max-width: 320px; }
.table-card { padding: 0; overflow: hidden; }
.tbl { width: 100%; border-collapse: collapse; font-size: 13px; }
.tbl th { text-align: left; padding: 12px 16px; background: #FAFAF8; color: var(--color-text-secondary); font-size: 12px; font-weight: 500; border-bottom: 1px solid var(--color-border); }
.tbl td { padding: 12px 16px; border-top: 1px solid var(--color-border); vertical-align: middle; }
.user-cell { display: flex; align-items: center; gap: 10px; }
.avatar { width: 36px; height: 36px; border-radius: 50%; display: flex; align-items: center; justify-content: center; color: #fff; font-size: 13px; font-weight: 600; flex-shrink: 0; }
.name { font-weight: 600; font-size: 13px; }
.sub { color: var(--color-text-secondary); font-size: 12px; }
.email { color: var(--color-text-secondary); font-size: 13px; }
.secondary { color: var(--color-text-secondary); font-size: 13px; }
.actions { display: flex; gap: 6px; align-items: center; }
.empty { text-align: center; color: var(--color-text-secondary); padding: 30px; }
.pagination-row { display: flex; justify-content: space-between; align-items: center; padding: 14px 16px; border-top: 1px solid var(--color-border); }
.count { font-size: 13px; color: var(--color-text-secondary); }

.modal-box { background: #fff; border-radius: 8px; padding: 28px; }
.modal-title { font-size: 18px; font-weight: 700; margin-bottom: 20px; padding-bottom: 16px; border-bottom: 1px solid var(--color-border); }
.grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.info-note { background: #F8F8F5; border: 1px solid var(--color-border); border-radius: 6px; padding: 10px 14px; font-size: 13px; color: var(--color-text-secondary); margin-bottom: 8px; }
.modal-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 20px; padding-top: 16px; border-top: 1px solid var(--color-border); }
</style>