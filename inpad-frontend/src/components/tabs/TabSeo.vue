<script setup lang="ts">
import { computed, ref } from 'vue'
import { NInput, NFormItem, NButton, useMessage } from 'naive-ui'
import type { ArchObject } from '@/api/types'
import { api } from '@/api/client'

const props = defineProps<{ form: ArchObject }>()
const message = useMessage()

const titleLen = computed(() => (props.form.seoTitle || '').length)
const descLen = computed(() => (props.form.seoDescription || '').length)

const ogInput = ref<HTMLInputElement | null>(null)
function pickOg() { ogInput.value?.click() }

async function handleOg(ev: Event) {
  const t = ev.target as HTMLInputElement
  const file = t.files?.[0]
  if (!file) return
  const fd = new FormData()
  fd.append('file', file)
  fd.append('archObjectId', String(props.form.id))
  fd.append('mediaType', 'PresentationCover')
  try {
    const m = (await api.uploadMedia(fd)) as { url?: string }
    if (m?.url) props.form.ogImageUrl = m.url
    message.success('OG image обновлён')
  } catch (e) {
    message.error((e as Error).message)
  }
  t.value = ''
}

function fullUrl(u?: string) {
  if (!u) return ''
  if (u.startsWith('http')) return u
  return `${api.baseUrl}${u.startsWith('/') ? '' : '/'}${u}`
}
</script>

<template>
  <div class="layout">
    <section class="card">
      <h3 class="card-title">SEO и метаданные</h3>

      <NFormItem label="SEO-заголовок">
        <NInput v-model:value="form.seoTitle" :maxlength="80" />
      </NFormItem>
      <div class="count">{{ titleLen }} символов · рекомендуется 50–60</div>

      <NFormItem label="Meta description" style="margin-top: 12px;">
        <NInput v-model:value="form.seoDescription" type="textarea" :rows="3" :maxlength="200" />
      </NFormItem>
      <div class="count">{{ descLen }} символов · рекомендуется 150–160</div>

      <NFormItem label="ЧПУ-адрес (Slug)" style="margin-top: 12px;">
        <div class="slug">
          <span class="prefix">inpad.ru/projects/</span>
          <NInput v-model:value="form.slug" placeholder="zhk-samolet" />
        </div>
      </NFormItem>

      <NFormItem label="Ключевые слова">
        <NInput v-model:value="form.seoKeywords" placeholder="через запятую" />
      </NFormItem>

      <NFormItem label="OG Image">
        <div class="og">
          <img v-if="form.ogImageUrl" :src="fullUrl(form.ogImageUrl)" alt="" />
          <div v-else class="og-placeholder">Не задано</div>
          <NButton size="small" @click="pickOg">Выбрать файл</NButton>
          <input ref="ogInput" type="file" accept="image/*" hidden @change="handleOg" />
        </div>
      </NFormItem>
    </section>

    <aside class="card">
      <h3 class="card-title">Превью в поиске</h3>
      <div class="preview">
        <div class="p-url">inpad.ru › projects › {{ form.slug || '—' }}</div>
        <div class="p-title">{{ form.seoTitle || form.name || 'Без заголовка' }}</div>
        <div class="p-desc">{{ form.seoDescription || form.shortDescription || 'Описание не задано' }}</div>
      </div>
    </aside>
  </div>
</template>

<style scoped>
.layout { display: grid; grid-template-columns: 2fr 1fr; gap: 20px; }
.count { font-size: 12px; color: var(--color-text-secondary); margin-top: -8px; }
.slug { display: flex; align-items: center; gap: 0; }
.slug .prefix { padding: 0 10px; background: #FAFAF8; border: 1px solid var(--color-border); border-right: none; border-radius: 6px 0 0 6px; font-size: 13px; color: var(--color-text-secondary); height: 34px; display: flex; align-items: center; }
.og { display: flex; align-items: center; gap: 12px; }
.og img { width: 120px; height: 70px; object-fit: cover; border-radius: 6px; border: 1px solid var(--color-border); }
.og-placeholder { width: 120px; height: 70px; border: 1px dashed var(--color-border); border-radius: 6px; display: flex; align-items: center; justify-content: center; color: var(--color-text-secondary); font-size: 12px; }

.preview { background: #fff; }
.p-url { font-size: 12px; color: #5f6368; }
.p-title { color: #1a0dab; font-size: 18px; line-height: 1.3; margin: 4px 0; cursor: pointer; }
.p-title:hover { text-decoration: underline; }
.p-desc { font-size: 13px; color: #4d5156; line-height: 1.5; }
@media (max-width: 1100px) { .layout { grid-template-columns: 1fr; } }
</style>