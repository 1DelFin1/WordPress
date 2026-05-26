<script setup lang="ts">
import { computed, ref } from 'vue'
import { NButton, NAlert, useMessage, useDialog } from 'naive-ui'
import type { ArchObject } from '@/api/types'
import StatusBadge from '@/components/StatusBadge.vue'
import { api } from '@/api/client'

const props = defineProps<{ form: ArchObject; audit?: { date: string; user: string; action: string }[] }>()
const emit = defineEmits<{ (e: 'updated', o: ArchObject): void }>()

const message = useMessage()
const dialog = useDialog()
const publishing = ref(false)

const checks = computed(() => {
  const f = props.form
  return [
    { label: 'Название объекта', ok: !!f.name },
    { label: 'Краткое описание', ok: !!f.shortDescription },
    { label: 'Полное описание', ok: !!f.fullDescription },
    { label: 'Тип объекта', ok: !!f.objectType },
    { label: 'Главное изображение', ok: f.media.some((m) => m.mediaType === 'MainImage') },
    { label: 'SEO-заголовок', ok: !!f.seoTitle },
    { label: 'Meta description', ok: !!f.seoDescription },
    { label: 'OG Image', ok: !!f.ogImageUrl },
  ]
})
const okCount = computed(() => checks.value.filter((c) => c.ok).length)
const allOk = computed(() => okCount.value === checks.value.length)

const wpUrl = computed(() => (props.form.wordPressPostId ? `https://inpad.ru/?p=${props.form.wordPressPostId}` : null))

const fmtDate = (s?: string) => (s ? new Date(s).toLocaleString('ru-RU') : '—')

async function publish() {
  publishing.value = true
  try {
    const o = await api.publishObject(props.form.id)
    emit('updated', o)
    message.success('Опубликовано')
  } catch (e) {
    message.error((e as Error).message)
  } finally {
    publishing.value = false
  }
}
function unpublish() {
  dialog.warning({
    title: 'Снять с публикации?',
    content: 'Объект станет недоступен на сайте',
    positiveText: 'Снять',
    negativeText: 'Отмена',
    onPositiveClick: async () => {
      try {
        const o = await api.unpublishObject(props.form.id)
        emit('updated', o)
        message.success('Снято с публикации')
      } catch (e) {
        message.error((e as Error).message)
      }
    },
  })
}
</script>

<template>
  <div class="layout">
    <section class="card">
      <h3 class="card-title">Публикация на сайте WordPress</h3>

      <div class="row">
        <div class="label-tiny">Статус объекта в системе</div>
        <StatusBadge :status="form.status" />
      </div>
      <div class="row">
        <div class="label-tiny">WordPress-статус</div>
        <div class="wp-row">
          <StatusBadge :status="form.wordPressStatus" kind="wordpress" />
          <a v-if="wpUrl" :href="wpUrl" target="_blank">{{ wpUrl }}</a>
        </div>
        <div class="text-secondary tiny">Обновлено: {{ fmtDate(form.publishedAt || form.updatedAt) }}</div>
      </div>

      <NAlert v-if="form.wordPressStatus === 'Published' || form.wordPressStatus === 'Updated'" type="success" :show-icon="false" class="alert">
        Объект успешно опубликован на сайте.
      </NAlert>
      <NAlert v-else-if="form.wordPressStatus === 'PublishError'" type="error" :show-icon="false" class="alert">
        Ошибка публикации. Проверьте обязательные поля.
      </NAlert>

      <div class="actions">
        <NButton type="primary" :loading="publishing" :disabled="!allOk" @click="publish">
          {{ form.wordPressStatus === 'Published' ? 'Обновить страницу' : 'Опубликовать' }}
        </NButton>
        <NButton v-if="form.wordPressStatus === 'Published' || form.wordPressStatus === 'Updated'" type="error" ghost @click="unpublish">Снять с публикации</NButton>
        <a v-if="wpUrl" :href="wpUrl" target="_blank" class="ext">Посмотреть ↗</a>
      </div>

      <div class="history">
        <div class="label-tiny">История публикаций</div>
        <table class="tbl">
          <thead><tr><th>Дата</th><th>Действие</th><th>Пользователь</th></tr></thead>
          <tbody>
            <tr v-if="!audit || audit.length === 0"><td colspan="3" class="empty">Нет записей</td></tr>
            <tr v-for="(e, i) in (audit || [])" :key="i">
              <td>{{ e.date }}</td>
              <td>{{ e.action }}</td>
              <td>{{ e.user }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <aside class="card">
      <h3 class="card-title">Готовность к публикации</h3>
      <ul class="checks">
        <li v-for="c in checks" :key="c.label" :class="{ ok: c.ok, bad: !c.ok }">
          <span class="mark">{{ c.ok ? '✓' : '✗' }}</span>
          {{ c.label }}
        </li>
      </ul>
      <div class="summary" :class="{ done: allOk }">
        {{ okCount }} из {{ checks.length }} полей заполнены
      </div>
    </aside>
  </div>
</template>

<style scoped>
.layout { display: grid; grid-template-columns: 2fr 1fr; gap: 20px; }
.row { margin-bottom: 14px; }
.wp-row { display: flex; align-items: center; gap: 12px; }
.tiny { font-size: 12px; margin-top: 4px; }
.alert { margin: 12px 0; }
.actions { display: flex; align-items: center; gap: 10px; margin: 12px 0 20px; }
.ext { margin-left: auto; font-size: 13px; }
.history { margin-top: 20px; }
.tbl { width: 100%; border-collapse: collapse; font-size: 13px; }
.tbl th, .tbl td { padding: 8px; text-align: left; border-bottom: 1px solid var(--color-border); }
.tbl th { color: var(--color-text-secondary); font-weight: 500; font-size: 12px; }
.empty { color: var(--color-text-secondary); text-align: center; padding: 16px; }

.checks { list-style: none; padding: 0; margin: 0 0 16px; }
.checks li { padding: 8px 0; font-size: 13px; display: flex; align-items: center; gap: 8px; border-top: 1px solid var(--color-border); }
.checks li:first-child { border-top: none; }
.checks li.ok .mark { color: #52C41A; }
.checks li.bad .mark { color: #F5222D; }
.mark { width: 18px; text-align: center; font-weight: 700; }
.summary { padding: 10px 12px; background: #FFF7E0; color: #B85A00; border-radius: 6px; font-size: 13px; font-weight: 500; }
.summary.done { background: #E8F8E0; color: #389E0D; }

@media (max-width: 1100px) { .layout { grid-template-columns: 1fr; } }
</style>