<script setup lang="ts">
import { computed, ref } from 'vue'
import { NButton, useMessage } from 'naive-ui'
import type { ArchObject, ObjectMedia } from '@/api/types'
import { api } from '@/api/client'

const props = defineProps<{ form: ArchObject }>()
const message = useMessage()

const mainImage = computed(() => props.form.media.find((m) => m.mediaType === 'MainImage'))
const gallery = computed(() =>
  props.form.media.filter((m) => m.mediaType !== 'MainImage').sort((a, b) => a.sortOrder - b.sortOrder),
)

const mainInput = ref<HTMLInputElement | null>(null)
const galleryInput = ref<HTMLInputElement | null>(null)

function pickMain() {
  mainInput.value?.click()
}
function pickGallery() {
  galleryInput.value?.click()
}

async function uploadFile(file: File, mediaType: string) {
  const fd = new FormData()
  fd.append('file', file)
  fd.append('archObjectId', String(props.form.id))
  fd.append('mediaType', mediaType)
  fd.append('useOnWebsite', 'true')
  fd.append('useInPresentation', 'true')
  fd.append('useInPortfolio', 'false')
  try {
    const m = (await api.uploadMedia(fd)) as ObjectMedia
    if (m && m.id) props.form.media.push(m)
    message.success('Загружено')
  } catch (e) {
    message.error((e as Error).message)
  }
}

async function handleMain(ev: Event) {
  const t = ev.target as HTMLInputElement
  const file = t.files?.[0]
  if (!file) return
  if (mainImage.value) {
    try {
      await api.deleteMedia(mainImage.value.id)
      const idx = props.form.media.indexOf(mainImage.value)
      if (idx >= 0) props.form.media.splice(idx, 1)
    } catch {
      /* ignore */
    }
  }
  await uploadFile(file, 'MainImage')
  t.value = ''
}

async function handleGallery(ev: Event) {
  const t = ev.target as HTMLInputElement
  const files = Array.from(t.files || [])
  for (const f of files) await uploadFile(f, 'Gallery')
  t.value = ''
}

async function removeMedia(m: ObjectMedia) {
  try {
    await api.deleteMedia(m.id)
    const i = props.form.media.indexOf(m)
    if (i >= 0) props.form.media.splice(i, 1)
    message.success('Удалено')
  } catch (e) {
    message.error((e as Error).message)
  }
}

async function toggleFlag(m: ObjectMedia, field: 'useOnWebsite' | 'useInPortfolio' | 'useInPresentation') {
  m[field] = !m[field]
  try {
    await api.updateMedia(m.id, {
      useOnWebsite: m.useOnWebsite,
      useInPortfolio: m.useInPortfolio,
      useInPresentation: m.useInPresentation,
    })
  } catch (e) {
    message.error((e as Error).message)
  }
}

function fullUrl(u: string) {
  if (!u) return ''
  if (u.startsWith('http')) return u
  return `${api.baseUrl}${u.startsWith('/') ? '' : '/'}${u}`
}
</script>

<template>
  <section class="card">
    <h3 class="card-title">Главное изображение — <span class="req">обязательное</span></h3>
    <div v-if="mainImage" class="main-row">
      <img :src="fullUrl(mainImage.url)" alt="" class="thumb" />
      <div class="meta">
        <div class="fn">{{ mainImage.fileName }}</div>
        <div class="fs">JPG · главное изображение</div>
      </div>
      <NButton size="small" @click="pickMain">Заменить</NButton>
      <NButton size="small" type="error" tertiary @click="removeMedia(mainImage)">Удалить</NButton>
    </div>
    <div class="drop" @click="pickMain">
      <div class="drop-icon">⬆</div>
      <div>Перетащите файл или нажмите для выбора</div>
      <div class="drop-hint">JPG, PNG, WebP · макс. 10 МБ</div>
    </div>
    <input ref="mainInput" type="file" accept="image/*" hidden @change="handleMain" />
  </section>

  <section class="card" style="margin-top: 20px;">
    <div class="head">
      <h3 class="card-title" style="margin: 0;">Галерея изображений</h3>
      <span class="hint">Порядок = порядку на сайте</span>
    </div>

    <div class="grid">
      <div v-for="m in gallery" :key="m.id" class="g-card">
        <img :src="fullUrl(m.url)" :alt="m.title || m.fileName" />
        <div class="g-meta">
          <div class="g-fn" :title="m.fileName">{{ m.fileName }}</div>
          <div class="g-actions">
            <button class="pill" :class="{ active: m.useOnWebsite }" @click="toggleFlag(m, 'useOnWebsite')">На сайт</button>
            <button class="pill" :class="{ active: m.useInPortfolio }" @click="toggleFlag(m, 'useInPortfolio')">Портфолио</button>
            <button class="x" @click="removeMedia(m)">×</button>
          </div>
        </div>
      </div>

      <div class="drop g-drop" @click="pickGallery">
        <div class="drop-icon">+</div>
        <div>Добавить</div>
      </div>
    </div>
    <input ref="galleryInput" type="file" accept="image/*" multiple hidden @change="handleGallery" />
  </section>
</template>

<style scoped>
.req { color: #F5222D; font-weight: 500; font-size: 13px; }
.main-row { display: flex; align-items: center; gap: 16px; padding: 12px; background: #FAFAF8; border-radius: 8px; margin-bottom: 12px; }
.thumb { width: 88px; height: 88px; object-fit: cover; border-radius: 6px; }
.meta { flex: 1; }
.fn { font-weight: 600; }
.fs { color: var(--color-text-secondary); font-size: 12px; }
.drop {
  border: 2px dashed var(--color-border);
  border-radius: 8px;
  padding: 24px;
  text-align: center;
  cursor: pointer;
  color: var(--color-text-secondary);
}
.drop:hover { border-color: var(--color-brand); color: var(--color-brand); }
.drop-icon { font-size: 24px; margin-bottom: 6px; }
.drop-hint { font-size: 12px; margin-top: 6px; }
.head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
.hint { font-size: 12px; color: var(--color-text-secondary); }
.grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px; }
.g-card { border: 1px solid var(--color-border); border-radius: 8px; overflow: hidden; background: #fff; }
.g-card img { width: 100%; height: 140px; object-fit: cover; display: block; }
.g-meta { padding: 8px; }
.g-fn { font-size: 12px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; margin-bottom: 6px; }
.g-actions { display: flex; gap: 4px; align-items: center; }
.pill { font-size: 11px; padding: 2px 8px; border: 1px solid var(--color-border); border-radius: 999px; background: #fff; cursor: pointer; }
.pill.active { background: var(--color-brand); color: #fff; border-color: var(--color-brand); }
.x { margin-left: auto; border: none; background: transparent; color: #F5222D; cursor: pointer; font-size: 16px; }
.g-drop { min-height: 200px; display: flex; flex-direction: column; align-items: center; justify-content: center; }
</style>