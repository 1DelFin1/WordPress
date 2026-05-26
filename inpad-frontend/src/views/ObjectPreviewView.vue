<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { NButton, NSpin, useMessage } from 'naive-ui'
import { useAuthStore } from '@/stores/auth'
import { api } from '@/api/client'
import type { ArchObject } from '@/api/types'

const route = useRoute()
const router = useRouter()
const message = useMessage()
const auth = useAuthStore()

const id = computed(() => Number(route.params.id))
const obj = ref<ArchObject | null>(null)
const loading = ref(false)

onMounted(async () => {
  loading.value = true
  try {
    obj.value = await api.getObject(id.value)
  } catch (e) {
    message.error((e as Error).message)
  } finally { loading.value = false }
})

const mainImage = computed(() => obj.value?.media.find((m) => m.mediaType === 'MainImage'))
const gallery = computed(() => obj.value?.media.filter((m) => m.mediaType !== 'MainImage') || [])

function fullUrl(u?: string) {
  if (!u) return ''
  if (u.startsWith('http')) return u
  return `${api.baseUrl}${u.startsWith('/') ? '' : '/'}${u}`
}

async function publish() {
  if (!obj.value) return
  try {
    obj.value = await api.publishObject(obj.value.id)
    message.success('Опубликовано')
  } catch (e) {
    message.error((e as Error).message)
  }
}
function logout() { auth.logout(); router.push('/login') }
</script>

<template>
  <div class="wrap">
    <header class="topbar">
      <RouterLink to="/objects" class="brand">ИНПАД</RouterLink>
      <div class="title">Предпросмотр: {{ obj?.name }}</div>
      <div class="spacer" />
      <NButton @click="router.back()">← Назад к редактированию</NButton>
      <NButton type="primary" @click="publish">Опубликовать</NButton>
      <NButton quaternary @click="logout">Выйти</NButton>
    </header>

    <div class="info">
      Это предпросмотр страницы. Так объект будет выглядеть на сайте inpad.ru. Изменения не сохранены.
    </div>

    <NSpin :show="loading">
      <article v-if="obj" class="content">
        <div class="hero" :style="mainImage ? { backgroundImage: `url(${fullUrl(mainImage.url)})` } : {}">
          <div class="hero-overlay">
            <div class="tags">
              <span class="tag" v-if="obj.projectStatus">{{ obj.projectStatus }}</span>
              <span class="tag" v-if="obj.objectType">{{ obj.objectType }}</span>
              <span class="tag" v-if="obj.city">{{ obj.city }}</span>
            </div>
            <h1 class="h1">{{ obj.name }}</h1>
          </div>
        </div>

        <div class="meta-row">
          <span v-if="obj.yearStart">{{ obj.yearStart }}</span>
          <span v-if="obj.designStage">· {{ obj.designStage }}</span>
          <span v-if="obj.client">· {{ obj.client }}</span>
          <span v-if="obj.city">· {{ obj.city }}</span>
        </div>

        <div class="cols">
          <div class="left">
            <p class="short">{{ obj.shortDescription }}</p>
            <h3>Описание</h3>
            <p class="full">{{ obj.fullDescription }}</p>
            <h3 v-if="gallery.length">Фотографии</h3>
            <div v-if="gallery.length" class="gallery">
              <img v-for="m in gallery" :key="m.id" :src="fullUrl(m.url)" :alt="m.title || m.fileName" />
            </div>
          </div>
          <aside class="right">
            <div class="char-card">
              <h3 class="ch-title">Характеристики</h3>
              <table class="ch-tbl">
                <tbody>
                  <tr v-for="c in obj.characteristics" :key="c.id || c.key">
                    <td>{{ c.label || c.key }}</td>
                    <td><b>{{ c.value }} <span v-if="c.unit">{{ c.unit }}</span></b></td>
                  </tr>
                </tbody>
              </table>
            </div>
          </aside>
        </div>
      </article>
    </NSpin>
  </div>
</template>

<style scoped>
.wrap { background: #fff; min-height: 100vh; }
.topbar {
  position: sticky; top: 0; z-index: 10;
  background: #fff; border-bottom: 1px solid var(--color-border);
  padding: 0 24px; height: 56px;
  display: flex; align-items: center; gap: 14px;
}
.brand { background: var(--color-brand); color: #fff !important; padding: 5px 12px; border-radius: 6px; font-weight: 700; font-size: 12px; letter-spacing: 0.08em; text-decoration: none; }
.title { font-weight: 600; }
.spacer { flex: 1; }
.info { background: #F2F2EE; color: var(--color-text-secondary); padding: 10px 24px; font-size: 13px; }
.content { padding: 0; max-width: 100%; }
.hero {
  height: 480px;
  background-color: #C8C8C0;
  background-size: cover;
  background-position: center;
  position: relative;
  display: flex; align-items: flex-end;
}
.hero-overlay { width: 100%; padding: 40px 80px; background: linear-gradient(0deg, rgba(0,0,0,0.6), rgba(0,0,0,0)); color: #fff; }
.tags { display: flex; gap: 8px; margin-bottom: 12px; }
.tag { background: rgba(255,255,255,0.2); border: 1px solid rgba(255,255,255,0.3); padding: 3px 10px; border-radius: 999px; font-size: 12px; }
.h1 { font-size: 40px; font-weight: 700; margin: 0; line-height: 1.1; }
.meta-row { padding: 20px 80px; color: var(--color-text-secondary); font-size: 14px; }
.cols { display: grid; grid-template-columns: 2fr 1fr; gap: 40px; padding: 0 80px 60px; }
.short { font-size: 16px; line-height: 1.6; }
.left h3 { font-size: 22px; margin: 32px 0 12px; }
.full { line-height: 1.7; white-space: pre-wrap; }
.gallery { display: grid; grid-template-columns: repeat(4, 1fr); gap: 8px; }
.gallery img { width: 100%; height: 140px; object-fit: cover; border-radius: 4px; }
.char-card { background: #FAFAF8; border-radius: 8px; padding: 20px; position: sticky; top: 80px; }
.ch-title { margin: 0 0 12px; font-size: 16px; }
.ch-tbl { width: 100%; font-size: 13px; border-collapse: collapse; }
.ch-tbl td { padding: 8px 4px; }
.ch-tbl tr:nth-child(odd) td { background: #fff; }
.ch-tbl td:last-child { text-align: right; }
@media (max-width: 1100px) {
  .cols { grid-template-columns: 1fr; padding: 0 24px 40px; }
  .hero-overlay, .meta-row { padding-left: 24px; padding-right: 24px; }
}
</style>