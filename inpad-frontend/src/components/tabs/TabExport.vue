<script setup lang="ts">
import { ref } from 'vue'
import { NButton, NCheckbox, useMessage } from 'naive-ui'
import type { ArchObject } from '@/api/types'
import { api } from '@/api/client'

const props = defineProps<{ form: ArchObject }>()
const message = useMessage()

type Format = 'pptx' | 'docx' | 'pdf' | 'txt' | 'json'
const format = ref<Format>('pptx')
const downloading = ref(false)

const formats: { value: Format; label: string; ext: string; disabled?: boolean }[] = [
  { value: 'pptx', label: 'PowerPoint', ext: 'PPTX' },
  { value: 'docx', label: 'Word', ext: 'DOCX' },
  { value: 'pdf', label: 'PDF', ext: 'PDF' },
  { value: 'txt', label: 'Текст', ext: 'TXT' },
  { value: 'json', label: 'JSON', ext: 'JSON', disabled: true },
]

const include = ref({
  main: true,
  description: true,
  characteristics: true,
  team: true,
  photos: true,
  seo: false,
})

const recent = ref<{ date: string; user: string; format: string; size: string }[]>([])

async function download() {
  if (format.value === 'json') return
  downloading.value = true
  try {
    const b = await api.exportObject(props.form.id, format.value)
    const url = URL.createObjectURL(b)
    const a = document.createElement('a')
    a.href = url
    a.download = `${props.form.name || 'object'}.${format.value}`
    document.body.appendChild(a)
    a.click()
    a.remove()
    URL.revokeObjectURL(url)

    const size = `${Math.round(b.size / 1024)} КБ`
    recent.value.unshift({
      date: new Date().toLocaleString('ru-RU'),
      user: 'Вы',
      format: format.value.toUpperCase(),
      size,
    })
    message.success('Скачано')
  } catch (e) {
    message.error((e as Error).message)
  } finally {
    downloading.value = false
  }
}
</script>

<template>
  <div class="layout">
    <section class="card">
      <h3 class="card-title">Экспорт данных объекта</h3>

      <div class="formats">
        <button
          v-for="f in formats"
          :key="f.value"
          class="fmt"
          :class="{ active: format === f.value, disabled: f.disabled }"
          :disabled="f.disabled"
          @click="format = f.value"
        >
          <div class="ext">{{ f.ext }}</div>
          <div class="lbl">{{ f.label }}</div>
        </button>
      </div>

      <div class="label-tiny" style="margin-top: 20px;">Состав экспорта</div>
      <div class="checks">
        <NCheckbox v-model:checked="include.main">Основные данные</NCheckbox>
        <NCheckbox v-model:checked="include.description">Описание</NCheckbox>
        <NCheckbox v-model:checked="include.characteristics">Характеристики</NCheckbox>
        <NCheckbox v-model:checked="include.team">Команда</NCheckbox>
        <NCheckbox v-model:checked="include.photos">Фотографии (до 20 шт.)</NCheckbox>
        <NCheckbox v-model:checked="include.seo">SEO-данные</NCheckbox>
      </div>

      <div class="actions">
        <NButton type="primary" :loading="downloading" :disabled="format === 'json'" @click="download">
          ↓ Скачать {{ format.toUpperCase() }}
        </NButton>
        <NButton>Настроить состав</NButton>
      </div>
    </section>

    <aside class="card">
      <h3 class="card-title">Последние выгрузки</h3>
      <ul v-if="recent.length" class="rlist">
        <li v-for="(r, i) in recent" :key="i">
          <div>
            <div class="r-date">{{ r.date }}</div>
            <div class="r-meta">{{ r.user }} · {{ r.format }} · {{ r.size }}</div>
          </div>
          <button class="dl">↓</button>
        </li>
      </ul>
      <div v-else class="empty">Пока нет выгрузок</div>
    </aside>
  </div>
</template>

<style scoped>
.layout { display: grid; grid-template-columns: 2fr 1fr; gap: 20px; }
.formats { display: flex; gap: 10px; flex-wrap: wrap; }
.fmt {
  flex: 1;
  min-width: 100px;
  background: #fff;
  border: 2px solid var(--color-border);
  border-radius: 8px;
  padding: 14px 8px;
  cursor: pointer;
  text-align: center;
}
.fmt:hover { border-color: var(--color-brand); }
.fmt.active { border-color: var(--color-brand); background: #F5F4EE; }
.fmt.disabled { opacity: 0.5; cursor: not-allowed; }
.ext { font-weight: 700; font-size: 14px; }
.lbl { font-size: 12px; color: var(--color-text-secondary); margin-top: 4px; }
.checks { display: flex; flex-direction: column; gap: 8px; margin-top: 8px; }
.actions { display: flex; gap: 10px; margin-top: 20px; }

.rlist { list-style: none; padding: 0; margin: 0; }
.rlist li { display: flex; justify-content: space-between; align-items: center; padding: 10px 0; border-top: 1px solid var(--color-border); }
.rlist li:first-child { border-top: none; }
.r-date { font-size: 13px; font-weight: 500; }
.r-meta { font-size: 12px; color: var(--color-text-secondary); }
.dl { border: none; background: transparent; color: var(--color-brand); cursor: pointer; font-size: 18px; }
.empty { color: var(--color-text-secondary); font-size: 13px; }

@media (max-width: 1100px) { .layout { grid-template-columns: 1fr; } }
</style>