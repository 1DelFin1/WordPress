<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { NButton, NInput, NModal, NSpin, useMessage, useDialog } from 'naive-ui'
import { api } from '@/api/client'
import type { ReferenceItem } from '@/api/types'

const route = useRoute()
const router = useRouter()
const message = useMessage()
const dialog = useDialog()

const NAV = [
  { key: 'object-types', label: 'Типы объектов' },
  { key: 'categories', label: 'Категории (теги)' },
  { key: 'cities', label: 'Города' },
  { key: 'inpad-roles', label: 'Роли компании' },
  { key: 'project-statuses', label: 'Статусы проектов' },
]

const type = computed(() => String(route.params.type || 'object-types'))
const currentLabel = computed(() => NAV.find((n) => n.key === type.value)?.label || type.value)

const items = ref<ReferenceItem[]>([])
const loading = ref(false)
const search = ref('')

const filtered = computed(() => {
  const q = search.value.toLowerCase().trim()
  if (!q) return items.value
  return items.value.filter((x) => (x.value || '').toLowerCase().includes(q))
})

async function load() {
  loading.value = true
  try {
    items.value = await api.listReferences(type.value)
  } catch (e) {
    items.value = []
    message.error((e as Error).message)
  } finally {
    loading.value = false
  }
}

onMounted(load)
watch(type, load)

const editing = ref<ReferenceItem | null>(null)
const editValue = ref('')
const modalOpen = ref(false)

function openAdd() {
  editing.value = null
  editValue.value = ''
  modalOpen.value = true
}
function openEdit(it: ReferenceItem) {
  editing.value = it
  editValue.value = it.value
  modalOpen.value = true
}
async function save() {
  if (!editValue.value.trim()) return
  try {
    if (editing.value) {
      await api.updateReference(type.value, editing.value.id, { value: editValue.value.trim() })
      message.success('Сохранено')
    } else {
      await api.createReference(type.value, { value: editValue.value.trim() })
      message.success('Добавлено')
    }
    modalOpen.value = false
    await load()
  } catch (e) {
    message.error((e as Error).message)
  }
}

function confirmDelete(it: ReferenceItem) {
  dialog.warning({
    title: 'Удалить значение?',
    content: `Будет удалено «${it.value}».`,
    positiveText: 'Удалить',
    negativeText: 'Отмена',
    onPositiveClick: async () => {
      try {
        await api.deleteReference(type.value, it.id)
        message.success('Удалено')
        await load()
      } catch (e) {
        message.error((e as Error).message)
      }
    },
  })
}

function fmtDate(s?: string) {
  if (!s) return '—'
  return new Date(s).toLocaleDateString('ru-RU')
}

function go(key: string) {
  router.push(`/references/${key}`)
}
</script>

<template>
  <div class="layout">
    <aside class="side">
      <ul class="nav">
        <li
          v-for="n in NAV"
          :key="n.key"
          :class="{ active: n.key === type }"
          @click="go(n.key)"
        >{{ n.label }}</li>
      </ul>
    </aside>

    <section class="main">
      <div class="head">
        <h1 class="h1">{{ currentLabel }}</h1>
        <div class="head-r">
          <NInput v-model:value="search" placeholder="Поиск" clearable style="width: 240px" />
          <NButton type="primary" @click="openAdd">+ Добавить</NButton>
        </div>
      </div>

      <NSpin :show="loading">
        <div class="card table-card">
          <table class="tbl">
            <thead>
              <tr>
                <th style="width: 30px"></th>
                <th>Значение</th>
                <th>Используется в объектах</th>
                <th>Дата создания</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="filtered.length === 0">
                <td colspan="5" class="empty">Нет записей</td>
              </tr>
              <tr v-for="it in filtered" :key="it.id">
                <td><span class="dot" /></td>
                <td>{{ it.value }}</td>
                <td><span class="unused">Не используется</span></td>
                <td>{{ fmtDate(it.createdAt) }}</td>
                <td class="acts">
                  <NButton size="small" tertiary @click="openEdit(it)">Изменить</NButton>
                  <NButton size="small" tertiary type="error" @click="confirmDelete(it)">×</NButton>
                </td>
              </tr>
              <tr class="add-hint" @click="openAdd">
                <td colspan="5">+ Добавить значение</td>
              </tr>
            </tbody>
          </table>
        </div>
      </NSpin>
    </section>

    <NModal v-model:show="modalOpen" preset="dialog" :title="editing ? 'Изменить' : 'Добавить'" positive-text="Сохранить" negative-text="Отмена" @positive-click="save">
      <NInput v-model:value="editValue" placeholder="Значение" @keyup.enter="save" autofocus />
    </NModal>
  </div>
</template>

<style scoped>
.layout { display: grid; grid-template-columns: 220px 1fr; gap: 20px; max-width: 1400px; margin: 0 auto; }
.side { background: #fff; border: 1px solid var(--color-border); border-radius: 8px; padding: 8px; height: fit-content; }
.nav { list-style: none; padding: 0; margin: 0; }
.nav li {
  padding: 10px 14px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 13px;
  margin-bottom: 2px;
  border-left: 3px solid transparent;
}
.nav li:hover { background: #FAFAF8; }
.nav li.active { background: var(--color-bg); font-weight: 600; border-left-color: var(--color-brand); }
.main { display: flex; flex-direction: column; gap: 16px; }
.head { display: flex; align-items: center; justify-content: space-between; }
.h1 { font-size: 22px; margin: 0; font-weight: 700; }
.head-r { display: flex; gap: 10px; align-items: center; }
.table-card { padding: 0; overflow: hidden; }
.tbl { width: 100%; border-collapse: collapse; font-size: 13px; }
.tbl th { text-align: left; padding: 14px 16px; background: #FAFAF8; font-weight: 600; color: var(--color-text-secondary); font-size: 12px; text-transform: uppercase; }
.tbl td { padding: 14px 16px; border-top: 1px solid var(--color-border); }
.dot { display: inline-block; width: 8px; height: 8px; border-radius: 999px; background: var(--color-brand); }
.unused { color: #F5222D; font-size: 12px; }
.acts { display: flex; justify-content: flex-end; gap: 6px; }
.empty { text-align: center; color: var(--color-text-secondary); padding: 30px; }
.add-hint td { color: var(--color-text-secondary); text-align: center; cursor: pointer; border-top: 1px dashed var(--color-border); }
.add-hint:hover td { background: #FAFAF8; color: var(--color-brand); }
</style>