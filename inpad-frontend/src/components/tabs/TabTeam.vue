<script setup lang="ts">
import { computed, ref } from 'vue'
import { NInput, NButton, NFormItem } from 'naive-ui'
import type { ArchObject, ObjectTeamMember } from '@/api/types'

const props = defineProps<{ form: ArchObject }>()

const ROLES = [
  { key: 'Руководитель проекта', required: true },
  { key: 'Главный архитектор' },
  { key: 'Инженер' },
  { key: 'BIM-специалист' },
  { key: 'ГИП' },
  { key: 'Дизайн интерьеров' },
]

function getMember(role: string) {
  return props.form.teamMembers.find((m) => m.role === role)
}
function getName(role: string) {
  return getMember(role)?.name || ''
}
function setName(role: string, name: string) {
  let m = getMember(role)
  if (!m && name) {
    m = { id: 0, name, role, sortOrder: props.form.teamMembers.length }
    props.form.teamMembers.push(m)
  } else if (m) {
    if (!name) {
      const i = props.form.teamMembers.indexOf(m)
      if (i >= 0) props.form.teamMembers.splice(i, 1)
    } else {
      m.name = name
    }
  }
}

const architects = computed(() =>
  props.form.teamMembers.filter((m) => m.role === 'Архитектор').map((m) => m.name).join(', '),
)
function setArchitects(v: string) {
  // remove all "Архитектор" entries and re-add
  props.form.teamMembers = props.form.teamMembers.filter((m) => m.role !== 'Архитектор')
  const names = v.split(',').map((s) => s.trim()).filter(Boolean)
  for (const n of names) {
    props.form.teamMembers.push({ id: 0, name: n, role: 'Архитектор', sortOrder: props.form.teamMembers.length })
  }
}

const partners = computed(() => props.form.teamMembers.filter((m) => m.role === 'Партнёр'))
const newPartner = ref('')
function addPartner() {
  const v = newPartner.value.trim()
  if (!v) return
  props.form.teamMembers.push({ id: 0, name: v, role: 'Партнёр', sortOrder: props.form.teamMembers.length })
  newPartner.value = ''
}
function removePartner(m: ObjectTeamMember) {
  const i = props.form.teamMembers.indexOf(m)
  if (i >= 0) props.form.teamMembers.splice(i, 1)
}
</script>

<template>
  <div class="layout">
    <section class="card">
      <h3 class="card-title">Команда проекта</h3>
      <div class="grid-2">
        <NFormItem v-for="r in ROLES" :key="r.key" :label="r.required ? `${r.key} *` : r.key">
          <NInput :value="getName(r.key)" @update:value="(v: string) => setName(r.key, v)" placeholder="ФИО" />
        </NFormItem>
      </div>
      <NFormItem label="Архитекторы">
        <NInput :value="architects" @update:value="setArchitects" placeholder="ФИО через запятую" />
      </NFormItem>
    </section>

    <aside class="card">
      <h3 class="card-title">Внешние партнёры</h3>
      <ul v-if="partners.length" class="plist">
        <li v-for="p in partners" :key="p.id || p.name">
          <span>{{ p.name }}</span>
          <button class="x" @click="removePartner(p)">×</button>
        </li>
      </ul>
      <div v-else class="empty">Партнёры не добавлены</div>

      <div class="add-row">
        <NInput v-model:value="newPartner" placeholder="Название организации" @keyup.enter="addPartner" />
        <NButton type="primary" @click="addPartner">+ Добавить</NButton>
      </div>
    </aside>
  </div>
</template>

<style scoped>
.layout { display: grid; grid-template-columns: 2fr 1fr; gap: 20px; }
.plist { list-style: none; padding: 0; margin: 0 0 16px; }
.plist li { display: flex; align-items: center; justify-content: space-between; padding: 10px 12px; background: #FAFAF8; border-radius: 6px; margin-bottom: 6px; font-size: 13px; }
.x { border: none; background: transparent; cursor: pointer; color: #F5222D; font-size: 16px; }
.empty { color: var(--color-text-secondary); font-size: 13px; margin-bottom: 16px; }
.add-row { display: flex; gap: 8px; }
@media (max-width: 1100px) { .layout { grid-template-columns: 1fr; } }
</style>