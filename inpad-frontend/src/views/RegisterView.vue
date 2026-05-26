<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { NInput, NButton, NAlert, NSelect, NCheckbox, NFormItem } from 'naive-ui'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const router = useRouter()

const form = reactive({
  lastName: '',
  firstName: '',
  middleName: '',
  email: '',
  password: '',
  passwordConfirm: '',
  login: '',
  role: 'Editor',
  consent: false,
})

const error = ref('')
const success = ref(false)
const loading = ref(false)

const roleOptions = [
  { label: 'Редактор', value: 'Editor' },
  { label: 'Менеджер', value: 'Manager' },
  { label: 'Просмотрщик', value: 'Viewer' },
]

async function submit() {
  error.value = ''
  if (form.password !== form.passwordConfirm) {
    error.value = 'Пароли не совпадают'
    return
  }
  if (!form.consent) {
    error.value = 'Необходимо согласие на обработку данных'
    return
  }
  loading.value = true
  try {
    const fullName = [form.lastName, form.firstName, form.middleName].filter(Boolean).join(' ')
    await auth.register({
      email: form.email,
      password: form.password,
      name: fullName,
      login: form.login,
      role: form.role,
    })
    success.value = true
    setTimeout(() => router.push('/login'), 1500)
  } catch (e) {
    error.value = (e as Error).message || 'Не удалось зарегистрироваться'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="card">
    <h1 class="title">Регистрация</h1>
    <p class="subtitle">Заполните все обязательные поля</p>

    <div class="grid">
      <NFormItem label="Фамилия *">
        <NInput v-model:value="form.lastName" placeholder="Иванов" />
      </NFormItem>
      <NFormItem label="Email *">
        <NInput v-model:value="form.email" placeholder="user@inpad.ru" />
      </NFormItem>
      <NFormItem label="Имя *">
        <NInput v-model:value="form.firstName" placeholder="Иван" />
      </NFormItem>
      <NFormItem label="Пароль *">
        <NInput v-model:value="form.password" type="password" show-password-on="click" />
      </NFormItem>
      <NFormItem label="Отчество">
        <NInput v-model:value="form.middleName" />
      </NFormItem>
      <NFormItem label="Подтверждение пароля *">
        <NInput v-model:value="form.passwordConfirm" type="password" show-password-on="click" />
      </NFormItem>
    </div>

    <NFormItem label="Логин *">
      <NInput v-model:value="form.login" />
    </NFormItem>

    <NFormItem label="Роль *">
      <NSelect v-model:value="form.role" :options="roleOptions" />
    </NFormItem>

    <NCheckbox v-model:checked="form.consent" class="consent">
      Согласен(-на) на обработку персональных данных
    </NCheckbox>

    <NAlert v-if="error" type="error" :show-icon="false" class="alert">{{ error }}</NAlert>
    <NAlert v-if="success" type="success" :show-icon="false" class="alert">
      Регистрация прошла успешно. Теперь вы можете войти в систему.
    </NAlert>

    <NButton type="primary" block size="large" :loading="loading" @click="submit">
      Зарегистрироваться
    </NButton>

    <p class="hint">
      <RouterLink to="/login">Уже есть аккаунт? Войти →</RouterLink>
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
  max-width: 560px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.04);
}
.title { font-size: 22px; font-weight: 700; margin: 0 0 6px; }
.subtitle { color: var(--color-text-secondary); font-size: 13px; margin: 0 0 20px; }
.grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0 16px; }
.consent { margin: 8px 0 16px; }
.alert { margin-bottom: 14px; }
.hint { margin-top: 18px; text-align: center; font-size: 13px; }
</style>