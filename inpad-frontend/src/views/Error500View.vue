<script setup lang="ts">
import { useRouter, useRoute } from 'vue-router'
import { NButton } from 'naive-ui'
const router = useRouter()
const route = useRoute()
const errorDetail = route.query.detail as string | undefined
</script>

<template>
  <div class="error-page">
    <div class="icon-wrap" style="background:#FFF2F0">
      <span class="icon">⚡</span>
    </div>
    <div class="code">500</div>
    <div class="title">Ошибка публикации</div>
    <div class="desc">Не удалось опубликовать объект в WordPress.<br>Проверьте подключение и попробуйте снова.</div>
    <div class="actions">
      <NButton type="primary" @click="router.go(-1)">Повторить попытку</NButton>
      <NButton @click="router.go(0)">Обновить страницу</NButton>
      <NButton>Написать администратору</NButton>
    </div>
    <div v-if="errorDetail" class="error-detail">
      <div class="detail-label">Детали ошибки:</div>
      <div>{{ errorDetail }}</div>
    </div>
  </div>
</template>

<style scoped>
.error-page { display: flex; flex-direction: column; align-items: center; justify-content: center; min-height: 70vh; text-align: center; padding: 40px; }
.icon-wrap { width: 100px; height: 100px; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-bottom: 24px; }
.icon { font-size: 40px; }
.code { font-size: 72px; font-weight: 700; color: #D0D0C8; line-height: 1; margin-bottom: 12px; }
.title { font-size: 22px; font-weight: 700; margin-bottom: 10px; }
.desc { font-size: 14px; color: var(--color-text-secondary); line-height: 1.6; margin-bottom: 28px; }
.actions { display: flex; gap: 12px; flex-wrap: wrap; justify-content: center; margin-bottom: 24px; }
.error-detail { background: #FFF2F0; border: 1px solid #FFCCC7; border-radius: 6px; padding: 12px 16px; font-size: 13px; color: #CF1322; max-width: 500px; text-align: left; }
.detail-label { font-weight: 600; margin-bottom: 4px; }
</style>