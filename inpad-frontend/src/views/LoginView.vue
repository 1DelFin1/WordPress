<script setup lang="ts">
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { NInput, NButton, NAlert, NForm, NFormItem } from 'naive-ui'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

const email = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

async function submit() {
  error.value = ''
  loading.value = true
  try {
    await auth.login(email.value, password.value)
    const redirect = (route.query.redirect as string) || '/objects'
    router.push(redirect)
  } catch (e) {
    error.value = 'Неверный логин или пароль. Попробуйте ещё раз.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="card">
    <h1 class="title">Вход в систему</h1>
    <p class="subtitle">Введите данные для входа</p>

    <NForm @submit.prevent="submit">
      <NFormItem label="Логин *">
        <NInput v-model:value="email" placeholder="Введите логин" />
      </NFormItem>
      <NFormItem label="Пароль *">
        <NInput v-model:value="password" type="password" show-password-on="click" placeholder="••••••••" @keyup.enter="submit" />
      </NFormItem>

      <NAlert v-if="error" type="error" :show-icon="false" class="alert">
        {{ error }}
      </NAlert>

      <NButton type="primary" block size="large" :loading="loading" @click="submit">Войти</NButton>
    </NForm>

    <p class="hint">
      <RouterLink to="/register">Нет доступа? Обратитесь к администратору</RouterLink>
    </p>
  </div>
</template>

<style scoped>
.card {
  background: #fff;
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 40px;
  width: 100%;
  max-width: 420px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.04);
}
.title {
  font-size: 22px;
  font-weight: 700;
  margin: 0 0 6px;
}
.subtitle {
  color: var(--color-text-secondary);
  font-size: 13px;
  margin: 0 0 24px;
}
.alert { margin-bottom: 16px; }
.hint {
  margin-top: 18px;
  text-align: center;
  font-size: 13px;
  color: var(--color-text-secondary);
}
</style>