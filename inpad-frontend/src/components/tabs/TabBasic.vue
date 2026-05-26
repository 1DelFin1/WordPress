<script setup lang="ts">
import { computed } from 'vue'
import { NInput, NSelect, NFormItem, NInputNumber } from 'naive-ui'
import type { ArchObject } from '@/api/types'
import StatusBadge from '@/components/StatusBadge.vue'

const props = defineProps<{
  form: ArchObject
  refs: {
    cities: { label: string; value: string }[]
    objectTypes: { label: string; value: string }[]
    inpadRoles: { label: string; value: string }[]
    projectStatuses: { label: string; value: string }[]
    designStages: { label: string; value: string }[]
  }
  audit?: { date: string; user: string; action: string }[]
}>()

const fmtDate = (s?: string) => (s ? new Date(s).toLocaleString('ru-RU') : '—')

const wpUrl = computed(() => {
  if (!props.form.wordPressPostId) return null
  return `https://inpad.ru/?p=${props.form.wordPressPostId}`
})
</script>

<template>
  <div class="layout">
    <div class="main">
      <section class="card">
        <h3 class="card-title">Идентификация объекта</h3>
        <NFormItem label="Название объекта *">
          <NInput v-model:value="form.name" placeholder="Например: ЖК «Самолёт-Парк»" />
        </NFormItem>
        <div class="grid-2">
          <NFormItem label="Краткое название">
            <NInput v-model:value="form.shortName" />
          </NFormItem>
          <NFormItem label="Заказчик">
            <NInput v-model:value="form.client" />
          </NFormItem>
        </div>
        <div class="grid-3">
          <NFormItem label="Адрес">
            <NInput v-model:value="form.address" />
          </NFormItem>
          <NFormItem label="Город *">
            <NSelect v-model:value="form.city" :options="refs.cities" filterable clearable tag />
          </NFormItem>
          <NFormItem label="Год проектирования">
            <NInputNumber v-model:value="form.yearStart" :min="1900" :max="2100" style="width: 100%" />
          </NFormItem>
        </div>
        <div class="grid-2">
          <NFormItem label="Тип объекта *">
            <NSelect v-model:value="form.objectType" :options="refs.objectTypes" filterable clearable />
          </NFormItem>
          <NFormItem label="Роль ИНПАД *">
            <NSelect v-model:value="form.inpadRole" :options="refs.inpadRoles" filterable clearable />
          </NFormItem>
        </div>
      </section>

      <section class="card">
        <h3 class="card-title">Классификация</h3>
        <div class="grid-3">
          <NFormItem label="Статус проекта">
            <NSelect v-model:value="form.projectStatus" :options="refs.projectStatuses" clearable />
          </NFormItem>
          <NFormItem label="Стадия проектирования">
            <NSelect v-model:value="form.designStage" :options="refs.designStages" clearable />
          </NFormItem>
          <NFormItem label="Год реализации">
            <NInputNumber v-model:value="form.yearEnd" :min="1900" :max="2100" style="width: 100%" />
          </NFormItem>
        </div>
      </section>
    </div>

    <aside class="side">
      <section class="card">
        <h3 class="card-title">Публикация</h3>
        <div class="row">
          <div class="label-tiny">Статус в системе</div>
          <StatusBadge :status="form.status" />
        </div>
        <div class="row">
          <div class="label-tiny">WordPress</div>
          <StatusBadge :status="form.wordPressStatus" kind="wordpress" />
        </div>
        <div class="row">
          <div class="label-tiny">Обновлено</div>
          <div>{{ fmtDate(form.updatedAt) }}</div>
        </div>
        <a v-if="wpUrl" :href="wpUrl" target="_blank" class="link">Посмотреть на сайте ↗</a>
        <div class="row">
          <div class="label-tiny">Создан</div>
          <div>{{ form.createdBy || '—' }} · {{ fmtDate(form.createdAt) }}</div>
        </div>
      </section>

      <section class="card">
        <h3 class="card-title">История изменений</h3>
        <div v-if="!audit || audit.length === 0" class="empty">Нет записей</div>
        <ul v-else class="timeline">
          <li v-for="(e, i) in audit" :key="i">
            <div class="t-date">{{ e.date }}</div>
            <div class="t-action">{{ e.action }}</div>
            <div class="t-user">{{ e.user }}</div>
          </li>
        </ul>
      </section>
    </aside>
  </div>
</template>

<style scoped>
.layout { display: grid; grid-template-columns: 2fr 1fr; gap: 20px; }
.main, .side { display: flex; flex-direction: column; gap: 20px; }
.row { margin-bottom: 14px; }
.link { display: inline-block; margin: 4px 0 14px; font-size: 13px; }
.empty { color: var(--color-text-secondary); font-size: 13px; }
.timeline { list-style: none; padding: 0; margin: 0; }
.timeline li { padding: 10px 0; border-top: 1px solid var(--color-border); font-size: 13px; }
.timeline li:first-child { border-top: none; padding-top: 0; }
.t-date { font-size: 11px; color: var(--color-text-secondary); }
.t-action { font-weight: 500; margin: 2px 0; }
.t-user { color: var(--color-text-secondary); font-size: 12px; }

@media (max-width: 1100px) { .layout { grid-template-columns: 1fr; } }
</style>