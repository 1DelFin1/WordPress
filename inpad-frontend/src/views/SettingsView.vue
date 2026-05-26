<script setup lang="ts">
import { ref } from 'vue'
import { NButton, NInput, NSwitch, useMessage } from 'naive-ui'

const base = (import.meta.env.VITE_API_URL as string) || 'http://localhost:5126'
function authHeaders(): Record<string, string> {
  const t = localStorage.getItem('inpad_token')
  return t ? { Authorization: `Bearer ${t}`, 'Content-Type': 'application/json' } : { 'Content-Type': 'application/json' }
}

const message = useMessage()
const activeSection = ref('wordpress')

// WordPress settings
const wpForm = ref({ url: 'https://inpad.ru', login: '', appPassword: '', postType: 'objects', taxObjectType: 'inpad_object_type', taxCity: 'inpad_city' })
const wpConnected = ref(false)
const wpLastCheck = ref('')
const wpCheckLoading = ref(false)
const wpSaveLoading = ref(false)

async function loadWpSettings() {
  try {
    const r = await fetch(`${base}/api/settings/wordpress`, { headers: authHeaders() })
    if (r.ok) {
      const d = await r.json()
      wpForm.value.url = d.url || 'https://inpad.ru'
      wpForm.value.login = d.username || ''
      wpForm.value.postType = d.postType || 'objects'
    }
  } catch (_) { /* ignore */ }
}

async function checkConnection() {
  wpCheckLoading.value = true
  try {
    const r = await fetch(`${base}/api/settings/wordpress`, { headers: authHeaders() })
    if (r.ok) {
      wpConnected.value = true
      wpLastCheck.value = new Date().toLocaleString('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' })
      message.success('Соединение установлено')
    } else {
      wpConnected.value = false
      message.error('Нет соединения')
    }
  } catch (_) { wpConnected.value = false; message.error('Ошибка подключения') }
  finally { wpCheckLoading.value = false }
}

async function saveWpSettings() {
  wpSaveLoading.value = true
  try {
    const r = await fetch(`${base}/api/settings/wordpress`, {
      method: 'PUT',
      headers: authHeaders(),
      body: JSON.stringify({ url: wpForm.value.url, username: wpForm.value.login, appPassword: wpForm.value.appPassword || undefined, postType: wpForm.value.postType })
    })
    if (r.ok) { message.success('Настройки сохранены'); wpLastCheck.value = new Date().toLocaleString('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }) }
    else message.error('Ошибка сохранения')
  } catch (e) { message.error((e as Error).message) }
  finally { wpSaveLoading.value = false }
}

// Sync settings
const syncToggles = ref({
  autoSync: true,
  syncImages: true,
  syncSeo: true,
  syncTeam: false,
  emailOnError: true,
})

const syncLog = ref([
  { date: '12.05.2025 14:32', name: 'ЖК «Северный Парк»', status: 'success', text: 'Опубликован успешно' },
  { date: '12.05.2025 14:28', name: 'БЦ «Технополис»', status: 'error', text: 'Ошибка: нет соединения' },
  { date: '10.05.2025 11:15', name: 'Школа № 67', status: 'success', text: 'Обновлено успешно' },
])

loadWpSettings()
</script>

<template>
  <div class="page">
    <h1 class="h1">Настройки</h1>
    <p class="subtitle">Управление подключениями и параметрами системы</p>

    <div class="layout">
      <div class="sidebar">
        <div
          v-for="s in [
            { key: 'wordpress', label: 'Подключение WordPress' },
            { key: 'sync', label: 'Синхронизация' },
            { key: 'notifications', label: 'Уведомления' },
            { key: 'security', label: 'Безопасность' },
            { key: 'about', label: 'О системе' },
          ]"
          :key="s.key"
          class="sidebar-item"
          :class="{ active: activeSection === s.key }"
          @click="activeSection = s.key"
        >
          {{ s.label }}
        </div>
      </div>

      <!-- WordPress connection -->
      <div v-if="activeSection === 'wordpress'" class="content">
        <div class="content-inner">
          <h2 class="section-title">Подключение к WordPress</h2>

          <div v-if="wpConnected" class="connection-ok">
            <span class="dot green"></span>
            <div>
              <strong>Соединение установлено</strong>
              <span class="wp-info">inpad.ru · WordPress 8.4.2 · REST API активен</span>
            </div>
          </div>
          <div v-else-if="wpLastCheck" class="connection-fail">
            <span class="dot red"></span>
            <strong>Нет соединения</strong>
          </div>

          <div class="form-section">
            <div class="field">
              <label>URL сайта *</label>
              <NInput v-model:value="wpForm.url" placeholder="https://inpad.ru" />
            </div>
            <div class="grid-2">
              <div class="field">
                <label>Логин WordPress *</label>
                <NInput v-model:value="wpForm.login" placeholder="inpad_admin" />
              </div>
              <div class="field">
                <label>Application Password *</label>
                <NInput v-model:value="wpForm.appPassword" type="password" placeholder="••••••••••••••" show-password-on="click" />
                <div class="hint">Создайте Application Password в панели WordPress:<br>Пользователи → Ваш профиль → Пароли приложений</div>
              </div>
            </div>
            <div class="grid-2">
              <div class="field">
                <label>Таксономия для типов объектов</label>
                <NInput v-model:value="wpForm.taxObjectType" />
              </div>
              <div class="field">
                <label>Таксономия для городов</label>
                <NInput v-model:value="wpForm.taxCity" />
              </div>
            </div>
          </div>

          <div class="form-actions">
            <NButton :loading="wpCheckLoading" @click="checkConnection">Проверить соединение</NButton>
            <NButton type="primary" :loading="wpSaveLoading" @click="saveWpSettings">Сохранить настройки</NButton>
            <span v-if="wpLastCheck" class="last-check">Последняя проверка: {{ wpLastCheck }}</span>
          </div>
        </div>

        <div class="help-card">
          <div class="help-title">Как получить Application Password</div>
          <ol class="help-list">
            <li>Откройте панель управления WP</li>
            <li>Перейдите: Пользователи → Профиль</li>
            <li>Найдите раздел «Пароли приложений»</li>
            <li>Введите имя и нажмите «Добавить»</li>
          </ol>
        </div>
      </div>

      <!-- Sync -->
      <div v-else-if="activeSection === 'sync'" class="content content-full">
        <h2 class="section-title">Настройки синхронизации</h2>

        <div class="stats-row">
          <div class="stat-card">
            <div class="stat-num">47</div>
            <div class="stat-label">Всего объектов</div>
          </div>
          <div class="stat-card green">
            <div class="stat-num">28</div>
            <div class="stat-label">Опубликовано на сайте</div>
          </div>
          <div class="stat-card orange">
            <div class="stat-num">11</div>
            <div class="stat-label">Ожидают публикации</div>
          </div>
          <div class="stat-card red">
            <div class="stat-num">2</div>
            <div class="stat-label">Ошибки синхронизации</div>
          </div>
        </div>

        <div class="card" style="margin-bottom:20px">
          <div class="toggle-group-title">Параметры автосинхронизации</div>
          <div class="toggle-row">
            <span>Автоматическая синхронизация при сохранении</span>
            <NSwitch v-model:value="syncToggles.autoSync" />
          </div>
          <div class="toggle-row">
            <span>Синхронизировать изображения</span>
            <NSwitch v-model:value="syncToggles.syncImages" />
          </div>
          <div class="toggle-row">
            <span>Синхронизировать SEO-данные</span>
            <NSwitch v-model:value="syncToggles.syncSeo" />
          </div>
          <div class="toggle-row">
            <span>Синхронизировать команду проекта</span>
            <NSwitch v-model:value="syncToggles.syncTeam" />
          </div>
          <div class="toggle-row" style="border-bottom:none">
            <span>Уведомлять об ошибках по email</span>
            <NSwitch v-model:value="syncToggles.emailOnError" />
          </div>
        </div>

        <div class="card">
          <div class="toggle-group-title">Журнал синхронизации</div>
          <div v-for="(entry, i) in syncLog" :key="i" class="log-row">
            <span class="dot" :class="entry.status === 'success' ? 'green' : 'red'"></span>
            <span class="log-date">{{ entry.date }}</span>
            <span class="log-name">{{ entry.name }}</span>
            <span class="log-status" :class="entry.status">{{ entry.text }}</span>
          </div>
        </div>
      </div>

      <!-- Stub sections -->
      <div v-else class="content content-full">
        <h2 class="section-title">{{ activeSection === 'notifications' ? 'Уведомления' : activeSection === 'security' ? 'Безопасность' : 'О системе' }}</h2>
        <div class="card">
          <p style="color:var(--color-text-secondary); font-size:14px;">Раздел в разработке.</p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page { max-width: 1100px; margin: 0 auto; }
.h1 { font-size: 22px; font-weight: 700; margin: 0 0 4px; }
.subtitle { color: var(--color-text-secondary); font-size: 13px; margin: 0 0 20px; }

.layout { display: flex; gap: 0; background: #fff; border: 1px solid var(--color-border); border-radius: 8px; overflow: hidden; }

.sidebar { width: 200px; flex-shrink: 0; border-right: 1px solid var(--color-border); padding: 8px 0; }
.sidebar-item { padding: 10px 20px; font-size: 14px; cursor: pointer; color: var(--color-text); transition: background 0.15s; }
.sidebar-item:hover { background: #F8F8F5; }
.sidebar-item.active { background: #F0F0EC; font-weight: 600; border-left: 3px solid var(--color-brand); padding-left: 17px; }

.content { display: flex; gap: 24px; flex: 1; padding: 28px; }
.content-full { flex: 1; padding: 28px; flex-direction: column; display: flex; }
.content-inner { flex: 1; }
.section-title { font-size: 18px; font-weight: 700; margin: 0 0 20px; }

.connection-ok { display: flex; align-items: center; gap: 10px; background: #F6FFED; border: 1px solid #B7EB8F; border-radius: 6px; padding: 12px 16px; margin-bottom: 20px; font-size: 14px; }
.connection-fail { display: flex; align-items: center; gap: 10px; background: #FFF2F0; border: 1px solid #FFCCC7; border-radius: 6px; padding: 12px 16px; margin-bottom: 20px; font-size: 14px; }
.wp-info { color: var(--color-text-secondary); margin-left: 8px; font-size: 13px; }

.dot { width: 8px; height: 8px; border-radius: 50%; display: inline-block; flex-shrink: 0; }
.dot.green { background: #52C41A; }
.dot.red { background: #F5222D; }

.form-section { display: flex; flex-direction: column; gap: 14px; margin-bottom: 20px; }
.field { display: flex; flex-direction: column; gap: 5px; }
.field label { font-size: 13px; font-weight: 500; color: var(--color-text); }
.hint { font-size: 12px; color: var(--color-text-secondary); background: #F8F8F5; border-radius: 4px; padding: 8px 10px; margin-top: 4px; line-height: 1.5; }
.grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 14px; }

.form-actions { display: flex; align-items: center; gap: 12px; flex-wrap: wrap; }
.last-check { font-size: 13px; color: var(--color-text-secondary); margin-left: 4px; }

.help-card { width: 220px; flex-shrink: 0; background: #FAFAF8; border: 1px solid var(--color-border); border-radius: 8px; padding: 16px; height: fit-content; }
.help-title { font-size: 13px; font-weight: 700; margin-bottom: 12px; }
.help-list { font-size: 13px; color: var(--color-text-secondary); padding-left: 16px; line-height: 1.8; margin: 0; }

/* Sync */
.stats-row { display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px; margin-bottom: 20px; }
.stat-card { background: #fff; border: 1px solid var(--color-border); border-radius: 8px; padding: 16px 20px; }
.stat-card.green .stat-num { color: #52C41A; }
.stat-card.orange .stat-num { color: #FA8C16; }
.stat-card.red .stat-num { color: #F5222D; }
.stat-num { font-size: 28px; font-weight: 700; color: var(--color-text); }
.stat-label { font-size: 12px; color: var(--color-text-secondary); margin-top: 4px; }

.toggle-group-title { font-size: 14px; font-weight: 600; margin-bottom: 12px; }
.toggle-row { display: flex; justify-content: space-between; align-items: center; padding: 12px 0; border-bottom: 1px solid var(--color-border); font-size: 14px; }

.log-row { display: flex; align-items: center; gap: 12px; padding: 10px 0; border-bottom: 1px solid var(--color-border); font-size: 13px; }
.log-row:last-child { border-bottom: none; }
.log-date { color: var(--color-text-secondary); width: 140px; flex-shrink: 0; }
.log-name { flex: 1; }
.log-status.success { color: #52C41A; }
.log-status.error { color: #F5222D; }

.card { background: #fff; border: 1px solid var(--color-border); border-radius: 8px; padding: 20px 24px; }
</style>