<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { NTabs, NTabPane, NButton, NSpin, useMessage } from 'naive-ui'
import StatusBadge from '@/components/StatusBadge.vue'
import TabBasic from '@/components/tabs/TabBasic.vue'
import TabDescription from '@/components/tabs/TabDescription.vue'
import TabCharacteristics from '@/components/tabs/TabCharacteristics.vue'
import TabMedia from '@/components/tabs/TabMedia.vue'
import TabTeam from '@/components/tabs/TabTeam.vue'
import TabSeo from '@/components/tabs/TabSeo.vue'
import TabPublication from '@/components/tabs/TabPublication.vue'
import TabExport from '@/components/tabs/TabExport.vue'
import { api } from '@/api/client'
import type { ArchObject, AuditLogEntry } from '@/api/types'

const route = useRoute()
const router = useRouter()
const message = useMessage()

const isNew = computed(() => route.name === 'object-new' || route.params.id === 'new')
const id = computed(() => (isNew.value ? 0 : Number(route.params.id)))

const form = ref<ArchObject | null>(null)
const loading = ref(false)
const saving = ref(false)
const tab = ref('basic')

const refs = ref({
  cities: [] as { label: string; value: string }[],
  objectTypes: [] as { label: string; value: string }[],
  inpadRoles: [] as { label: string; value: string }[],
  projectStatuses: [] as { label: string; value: string }[],
  designStages: [] as { label: string; value: string }[],
})

const audit = ref<{ date: string; user: string; action: string }[]>([])

function emptyObject(): ArchObject {
  return {
    id: 0,
    name: '',
    status: 'Draft',
    wordPressStatus: 'NotPublished',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    media: [],
    characteristics: [],
    teamMembers: [],
    categories: [],
  }
}

async function loadRefs() {
  try {
    const [cities, types, roles, ps, ds] = await Promise.all([
      api.listReferences('cities').catch(() => []),
      api.listReferences('object-types').catch(() => []),
      api.listReferences('inpad-roles').catch(() => []),
      api.listReferences('project-statuses').catch(() => []),
      api.listReferences('design-stages').catch(() => []),
    ])
    const mp = (xs: { value: string }[]) => xs.map((x) => ({ label: x.value, value: x.value }))
    refs.value = {
      cities: mp(cities),
      objectTypes: mp(types),
      inpadRoles: mp(roles),
      projectStatuses: mp(ps),
      designStages: mp(ds),
    }
  } catch { /* ignore */ }
}

async function loadAudit(objectId: number) {
  try {
    const entries = await api.listAudit({ entityType: 'ArchObject', entityId: objectId })
    audit.value = entries.map((e: AuditLogEntry) => ({
      date: new Date(e.createdAt).toLocaleString('ru-RU'),
      user: e.userName || `User ${e.userId || ''}`,
      action: e.details || e.action,
    }))
  } catch { audit.value = [] }
}

async function load() {
  loading.value = true
  try {
    if (isNew.value) {
      form.value = emptyObject()
    } else {
      form.value = await api.getObject(id.value)
      await loadAudit(id.value)
    }
  } catch (e) {
    message.error((e as Error).message)
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  await loadRefs()
  await load()
})

async function saveDraft() {
  if (!form.value) return
  saving.value = true
  try {
    const payload = { ...form.value, status: form.value.status || 'Draft' }
    if (isNew.value || form.value.id === 0) {
      const created = await api.createObject(payload)
      message.success('Создано')
      router.replace(`/objects/${created.id}`)
    } else {
      const upd = await api.updateObject(form.value.id, payload)
      form.value = upd
      message.success('Сохранено')
    }
  } catch (e) {
    message.error((e as Error).message)
  } finally {
    saving.value = false
  }
}

async function saveChanges() { await saveDraft() }

function preview() {
  if (!form.value || form.value.id === 0) {
    message.info('Сначала сохраните объект')
    return
  }
  router.push(`/objects/${form.value.id}/preview`)
}

function onUpdated(o: ArchObject) {
  form.value = o
}
</script>

<template>
  <div class="page">
    <div class="topbar">
      <RouterLink to="/objects" class="brand">ИНПАД</RouterLink>
      <div class="crumb">
        <RouterLink to="/objects">Объекты</RouterLink>
        <span class="sep">/</span>
        <span class="cur">{{ form?.name || (isNew ? 'Новый объект' : '…') }}</span>
      </div>
      <StatusBadge v-if="form" :status="form.status" />
      <div class="spacer" />
      <NButton :loading="saving" @click="saveDraft">Сохранить черновик</NButton>
      <NButton @click="preview">Предпросмотр</NButton>
      <NButton type="primary" :loading="saving" @click="saveChanges">Сохранить изменения</NButton>
    </div>

    <NSpin :show="loading">
      <div v-if="form" class="tabs-wrap">
        <NTabs v-model:value="tab" type="line" animated size="medium">
          <NTabPane name="basic" tab="Основное">
            <TabBasic :form="form" :refs="refs" :audit="audit" />
          </NTabPane>
          <NTabPane name="description" tab="Описание">
            <TabDescription :form="form" />
          </NTabPane>
          <NTabPane name="characteristics" tab="Характеристики">
            <TabCharacteristics :form="form" />
          </NTabPane>
          <NTabPane name="media" tab="Медиа" :disabled="form.id === 0">
            <TabMedia :form="form" />
          </NTabPane>
          <NTabPane name="team" tab="Команда">
            <TabTeam :form="form" />
          </NTabPane>
          <NTabPane name="seo" tab="SEO">
            <TabSeo :form="form" />
          </NTabPane>
          <NTabPane name="publication" tab="Публикация" :disabled="form.id === 0">
            <TabPublication :form="form" :audit="audit" @updated="onUpdated" />
          </NTabPane>
          <NTabPane name="export" tab="Выгрузка" :disabled="form.id === 0">
            <TabExport :form="form" />
          </NTabPane>
        </NTabs>
      </div>
    </NSpin>
  </div>
</template>

<style scoped>
.page { max-width: 1400px; margin: 0 auto; }
.topbar {
  background: #fff;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 10px 14px;
  display: flex;
  align-items: center;
  gap: 14px;
  margin-bottom: 16px;
  position: sticky;
  top: 72px;
  z-index: 5;
}
.brand {
  background: var(--color-brand);
  color: #fff !important;
  padding: 5px 12px;
  border-radius: 6px;
  font-weight: 700;
  font-size: 12px;
  letter-spacing: 0.08em;
  text-decoration: none;
}
.crumb { display: flex; align-items: center; gap: 8px; font-size: 14px; }
.sep { color: var(--color-text-secondary); }
.cur { font-weight: 600; }
.spacer { flex: 1; }
.tabs-wrap { background: #fff; border: 1px solid var(--color-border); border-radius: 8px; padding: 0 24px 24px; }
:deep(.n-tabs-nav) { padding-top: 12px; }
</style>