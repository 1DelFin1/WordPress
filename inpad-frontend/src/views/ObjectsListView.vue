<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import {
  NButton,
  NInput,
  NSelect,
  NPagination,
  NDropdown,
  useMessage,
  useDialog,
} from 'naive-ui'
import StatusBadge from '@/components/StatusBadge.vue'
import { api } from '@/api/client'
import type { ArchObject } from '@/api/types'

const router = useRouter()
const message = useMessage()
const dialog = useDialog()

const items = ref<ArchObject[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const loading = ref(false)

const search = ref('')
const status = ref<string | null>(null)
const objectType = ref<string | null>(null)
const city = ref<string | null>(null)
const inpadRole = ref<string | null>(null)
const yearStart = ref<string | null>(null)

const statuses = [
  { label: 'Все', value: null },
  { label: 'Черновик', value: 'Draft' },
  { label: 'На проверке', value: 'UnderReview' },
  { label: 'Опубликован', value: 'Published' },
  { label: 'Архивный', value: 'Archived' },
  { label: 'Ошибка', value: 'PublishError' },
]

type Opt = { label: string; value: string }
const typeOptions = ref<Opt[]>([])
const cityOptions = ref<Opt[]>([])
const roleOptions = ref<Opt[]>([])
const yearOptions = computed<Opt[]>(() => {
  const opts: Opt[] = []
  const now = new Date().getFullYear()
  for (let y = now; y >= now - 12; y--) opts.push({ label: String(y), value: String(y) })
  return opts
})

async function load() {
  loading.value = true
  try {
    const res = await api.listObjects({
      search: search.value || undefined,
      status: status.value || undefined,
      objectType: objectType.value || undefined,
      city: city.value || undefined,
      yearStart: yearStart.value || undefined,
      page: page.value,
      pageSize: pageSize.value,
    })
    items.value = res.items
    total.value = res.totalCount
  } catch (e) {
    message.error((e as Error).message)
  } finally {
    loading.value = false
  }
}

async function loadFilters() {
  try {
    const [types, cities, roles] = await Promise.all([
      api.listReferences('object-types').catch(() => []),
      api.listReferences('cities').catch(() => []),
      api.listReferences('inpad-roles').catch(() => []),
    ])
    typeOptions.value = types.map(t => ({ label: t.value, value: t.value }))
    cityOptions.value = cities.map(t => ({ label: t.value, value: t.value }))
    roleOptions.value = roles.map(t => ({ label: t.value, value: t.value }))
  } catch {
    /* ignore */
  }
}

onMounted(() => {
  loadFilters()
  load()
})

let debounceId: number | undefined
watch([search, status, objectType, city, inpadRole, yearStart], () => {
  page.value = 1
  window.clearTimeout(debounceId)
  debounceId = window.setTimeout(load, 250)
})
watch(page, load)

function fmtDate(s?: string) {
  if (!s) return '—'
  return new Date(s).toLocaleDateString('ru-RU')
}

function openObject(id: number) {
  router.push(`/objects/${id}`)
}
function editObject(id: number) {
  router.push(`/objects/${id}`)
}

function rowMenu(o: ArchObject) {
  return [
    { label: 'Дублировать', key: 'duplicate' },
    { label: 'Удалить', key: 'delete', props: { style: 'color: #F5222D' } },
    { label: 'Открыть на сайте', key: 'view', disabled: !o.wordPressPostId },
  ]
}

async function handleMenu(key: string, o: ArchObject) {
  if (key === 'duplicate') {
    try {
      const c = await api.duplicateObject(o.id)
      message.success('Объект дублирован')
      router.push(`/objects/${c.id}`)
    } catch (e) {
      message.error((e as Error).message)
    }
  } else if (key === 'delete') {
    dialog.warning({
      title: 'Удалить объект?',
      content: `Будет удалён «${o.name}». Действие нельзя отменить.`,
      positiveText: 'Удалить',
      negativeText: 'Отмена',
      onPositiveClick: async () => {
        try {
          await api.deleteObject(o.id)
          message.success('Удалено')
          load()
        } catch (e) {
          message.error((e as Error).message)
        }
      },
    })
  }
}
</script>

<template>
  <div class="page">
    <div class="head">
      <div>
        <h1 class="h1">Объекты</h1>
        <p class="sub">{{ total }} объектов в системе</p>
      </div>
      <NButton type="primary" size="large" @click="router.push('/objects/new')">+ Создать объект</NButton>
    </div>

    <div class="filters">
      <NInput v-model:value="search" placeholder="Поиск по названию, адресу, заказчику…" clearable class="search" />
      <div class="chips">
        <button
          v-for="s in statuses"
          :key="String(s.value)"
          class="chip"
          :class="{ active: status === s.value }"
          @click="status = s.value"
        >{{ s.label }}</button>
      </div>
      <NSelect v-model:value="objectType" :options="typeOptions" placeholder="Все типы" clearable class="sel" />
      <NSelect v-model:value="city" :options="cityOptions" placeholder="Все города" clearable class="sel" />
      <NSelect v-model:value="yearStart" :options="yearOptions" placeholder="Любой" clearable class="sel" />
      <NSelect v-model:value="inpadRole" :options="roleOptions" placeholder="Все роли" clearable class="sel" />
    </div>

    <div class="table-wrap card">
      <table class="tbl">
        <thead>
          <tr>
            <th>Название</th>
            <th>Город</th>
            <th>Тип</th>
            <th>Статус</th>
            <th>Изменён</th>
            <th>Автор</th>
            <th>WP-статус</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="items.length === 0 && !loading">
            <td colspan="8" class="empty">Нет объектов</td>
          </tr>
          <tr v-for="o in items" :key="o.id">
            <td>
              <div class="name">{{ o.name }}</div>
              <div class="addr">{{ o.address || '—' }}</div>
            </td>
            <td>{{ o.city || '—' }}</td>
            <td>{{ o.objectType || '—' }}</td>
            <td><StatusBadge :status="o.status" /></td>
            <td>{{ fmtDate(o.updatedAt) }}</td>
            <td>{{ o.createdBy || '—' }}</td>
            <td><StatusBadge :status="o.wordPressStatus" kind="wordpress" /></td>
            <td class="actions">
              <NButton size="small" tertiary @click="openObject(o.id)">Открыть</NButton>
              <NButton size="small" tertiary @click="editObject(o.id)">Ред.</NButton>
              <NDropdown :options="rowMenu(o)" @select="(k: string) => handleMenu(k, o)">
                <NButton size="small" tertiary>⋯</NButton>
              </NDropdown>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="footer">
      <span class="sub">Показано {{ items.length }} из {{ total }} объектов</span>
      <NPagination v-model:page="page" :item-count="total" :page-size="pageSize" />
    </div>
  </div>
</template>

<style scoped>
.page { max-width: 1400px; margin: 0 auto; }
.head { display: flex; align-items: flex-end; justify-content: space-between; margin-bottom: 20px; }
.h1 { font-size: 24px; font-weight: 700; margin: 0; }
.sub { color: var(--color-text-secondary); font-size: 13px; margin: 4px 0 0; }
.filters {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  align-items: center;
  margin-bottom: 16px;
  background: #fff;
  padding: 12px;
  border-radius: 8px;
  border: 1px solid var(--color-border);
}
.search { width: 280px; }
.chips { display: flex; gap: 6px; }
.chip {
  background: transparent;
  border: 1px solid var(--color-border);
  border-radius: 999px;
  padding: 4px 12px;
  font-size: 12px;
  cursor: pointer;
  color: var(--color-text);
}
.chip.active { background: var(--color-brand); color: #fff; border-color: var(--color-brand); }
.sel { width: 150px; }

.table-wrap { padding: 0; overflow: hidden; }
.tbl { width: 100%; border-collapse: collapse; font-size: 13px; }
.tbl th { text-align: left; padding: 14px 16px; background: #FAFAF8; font-weight: 600; color: var(--color-text-secondary); font-size: 12px; text-transform: uppercase; letter-spacing: 0.04em; }
.tbl td { padding: 14px 16px; border-top: 1px solid var(--color-border); vertical-align: middle; }
.tbl tbody tr:hover { background: #FAFAF8; }
.name { font-weight: 600; }
.addr { color: var(--color-text-secondary); font-size: 12px; margin-top: 2px; }
.actions { display: flex; gap: 6px; justify-content: flex-end; }
.empty { text-align: center; color: var(--color-text-secondary); padding: 36px; }

.footer { display: flex; align-items: center; justify-content: space-between; margin-top: 16px; }
</style>