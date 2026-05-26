<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { NButton } from 'naive-ui'

const router = useRouter()
const auth = useAuthStore()

function logout() {
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <div class="app">
    <header class="topbar">
      <RouterLink to="/objects" class="brand">ИНПАД</RouterLink>
      <nav class="nav">
        <RouterLink to="/objects" class="nav-link" active-class="active">Объекты</RouterLink>
        <RouterLink to="/references" class="nav-link" active-class="active">Справочники</RouterLink>
        <RouterLink to="/users" class="nav-link" active-class="active">Пользователи</RouterLink>
        <RouterLink to="/settings" class="nav-link" active-class="active">Настройки</RouterLink>
      </nav>
      <div class="user">
        <span class="user-name">{{ auth.user?.name || auth.user?.email || '—' }}</span>
        <NButton size="small" quaternary @click="logout">Выйти</NButton>
      </div>
    </header>
    <main class="content">
      <slot />
    </main>
  </div>
</template>

<style scoped>
.app {
  min-height: 100vh;
  background: var(--color-bg);
}
.topbar {
  background: #fff;
  border-bottom: 1px solid var(--color-border);
  display: flex;
  align-items: center;
  padding: 0 24px;
  height: 56px;
  gap: 24px;
  position: sticky;
  top: 0;
  z-index: 10;
}
.brand {
  background: var(--color-brand);
  color: #fff !important;
  font-weight: 700;
  letter-spacing: 0.08em;
  padding: 6px 14px;
  border-radius: 6px;
  text-decoration: none;
  font-size: 13px;
}
.brand:hover { background: var(--color-brand-hover); text-decoration: none; }
.nav { display: flex; gap: 4px; flex: 1; }
.nav-link {
  padding: 8px 14px;
  border-radius: 6px;
  font-size: 14px;
  color: var(--color-text);
  text-decoration: none;
}
.nav-link:hover { background: var(--color-bg); text-decoration: none; }
.nav-link.active {
  background: var(--color-bg);
  font-weight: 600;
  color: var(--color-text);
}
.user { display: flex; align-items: center; gap: 12px; }
.user-name { font-size: 13px; color: var(--color-text); }
.content { padding: 24px; }
</style>