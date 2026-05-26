<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  status?: string
  kind?: 'object' | 'wordpress'
}>()

const objectMap: Record<string, { label: string; color: string; bg: string }> = {
  Draft: { label: 'Черновик', color: '#666', bg: '#EEE' },
  UnderReview: { label: 'На проверке', color: '#FA8C16', bg: '#FFF2E5' },
  NeedsRevision: { label: 'Требуется доработка', color: '#FAAD14', bg: '#FFF7E0' },
  Published: { label: 'Опубликован', color: '#fff', bg: '#52C41A' },
  Archived: { label: 'Архивный', color: '#666', bg: '#E5E5E0' },
  PublishError: { label: 'Ошибка', color: '#fff', bg: '#F5222D' },
}

const wpMap: Record<string, { label: string; color: string; bg: string }> = {
  NotPublished: { label: 'Не опубликовано', color: '#666', bg: '#EEE' },
  WordPressDraft: { label: 'Черновик WP', color: '#666', bg: '#EEE' },
  Published: { label: 'Опубликовано', color: '#fff', bg: '#52C41A' },
  Updated: { label: 'Обновлено', color: '#fff', bg: '#52C41A' },
  Unpublished: { label: 'Снято', color: '#666', bg: '#EEE' },
  PublishError: { label: 'Ошибка', color: '#fff', bg: '#F5222D' },
}

const data = computed(() => {
  const k = props.kind || 'object'
  const map = k === 'wordpress' ? wpMap : objectMap
  const s = props.status || ''
  return (
    map[s] || { label: s || '—', color: '#666', bg: '#EEE' }
  )
})
</script>

<template>
  <span class="badge" :style="{ background: data.bg, color: data.color }">
    <span class="dot" v-if="data.color !== '#fff'" :style="{ background: data.color }"></span>
    {{ data.label }}
  </span>
</template>

<style scoped>
.badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 2px 10px;
  border-radius: 999px;
  font-size: 12px;
  line-height: 18px;
  font-weight: 500;
  white-space: nowrap;
}
.dot {
  width: 6px; height: 6px;
  border-radius: 999px;
  display: inline-block;
}
</style>