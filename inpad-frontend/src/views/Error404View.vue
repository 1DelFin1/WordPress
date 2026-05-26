<script setup lang="ts">
import { useRouter } from 'vue-router'
import { NButton } from 'naive-ui'
const router = useRouter()
</script>

<template>
  <div class="error-page">
    <div class="icon-wrap" style="background:#F5F5F5">
      <span class="icon">🔍</span>
    </div>
    <div class="code">404</div>
    <div class="title">Объект не найден</div>
    <div class="desc">Запрашиваемый объект был удалён, перемещён<br>или никогда не существовал в системе.</div>
    <div class="actions">
      <NButton type="primary" @click="router.push('/objects')">К списку объектов</NButton>
      <NButton @click="router.go(0)">Обновить страницу</NButton>
      <NButton>Написать администратору</NButton>
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
.actions { display: flex; gap: 12px; flex-wrap: wrap; justify-content: center; }
</style>