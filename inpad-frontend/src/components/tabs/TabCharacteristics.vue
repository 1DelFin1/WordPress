<script setup lang="ts">
import { computed } from 'vue'
import { NInput, NButton, NFormItem } from 'naive-ui'
import type { ArchObject, ObjectCharacteristic } from '@/api/types'

const props = defineProps<{ form: ArchObject }>()

const PREDEFINED = [
  { key: 'total_area', label: 'Общая площадь', unit: 'м²' },
  { key: 'site_area', label: 'Площадь участка', unit: 'м²' },
  { key: 'building_area', label: 'Площадь застройки', unit: 'м²' },
  { key: 'park_area', label: 'Площадь парка', unit: 'м²' },
  { key: 'floors', label: 'Этажность', unit: '' },
  { key: 'buildings_count', label: 'Кол-во корпусов', unit: '' },
  { key: 'units_count', label: 'Кол-во помещений', unit: '' },
  { key: 'parking', label: 'Парковочных мест', unit: '' },
  { key: 'arch_style', label: 'Архитектурный стиль', unit: '' },
  { key: 'housing_class', label: 'Класс жилья', unit: '' },
  { key: 'heating_type', label: 'Тип отопления', unit: '' },
  { key: 'facade_material', label: 'Материал фасада', unit: '' },
  { key: 'foundation_type', label: 'Тип фундамента', unit: '' },
  { key: 'elevators', label: 'Лифты', unit: '' },
]

const predefinedKeys = PREDEFINED.map((p) => p.key)

function ensureChar(key: string, label: string, unit: string): ObjectCharacteristic {
  let c = props.form.characteristics.find((x) => x.key === key)
  if (!c) {
    c = { id: 0, key, label, unit, value: '', sortOrder: props.form.characteristics.length } as ObjectCharacteristic
    props.form.characteristics.push(c)
  }
  return c
}

function getValue(key: string) {
  return props.form.characteristics.find((c) => c.key === key)?.value || ''
}
function setValue(key: string, label: string, unit: string, v: string) {
  const c = ensureChar(key, label, unit)
  c.value = v
}

const customChars = computed(() =>
  props.form.characteristics.filter((c) => !predefinedKeys.includes(c.key)),
)

function addCustom() {
  props.form.characteristics.push({
    id: 0,
    key: `custom_${Date.now()}`,
    label: '',
    value: '',
    unit: '',
    sortOrder: props.form.characteristics.length,
  })
}
function removeCustom(c: ObjectCharacteristic) {
  const i = props.form.characteristics.indexOf(c)
  if (i >= 0) props.form.characteristics.splice(i, 1)
}

function renderField(p: { key: string; label: string; unit: string }) {
  return {
    label: p.unit ? `${p.label} (${p.unit}) ${p.key === 'total_area' ? '*' : ''}`.trim() : p.label,
    value: getValue(p.key),
    set: (v: string) => setValue(p.key, p.label, p.unit, v),
  }
}
</script>

<template>
  <section class="card">
    <h3 class="card-title">Технико-экономические показатели</h3>
    <div class="grid-4">
      <template v-for="p in PREDEFINED.slice(0, 4)" :key="p.key">
        <NFormItem :label="renderField(p).label">
          <NInput :value="renderField(p).value" @update:value="(v: string) => setValue(p.key, p.label, p.unit, v)" />
        </NFormItem>
      </template>
    </div>
    <div class="grid-4">
      <template v-for="p in PREDEFINED.slice(4, 8)" :key="p.key">
        <NFormItem :label="renderField(p).label">
          <NInput :value="renderField(p).value" @update:value="(v: string) => setValue(p.key, p.label, p.unit, v)" />
        </NFormItem>
      </template>
    </div>
  </section>

  <section class="card" style="margin-top: 20px;">
    <h3 class="card-title">Дополнительные характеристики</h3>
    <div class="grid-4">
      <template v-for="p in PREDEFINED.slice(8, 12)" :key="p.key">
        <NFormItem :label="renderField(p).label">
          <NInput :value="renderField(p).value" @update:value="(v: string) => setValue(p.key, p.label, p.unit, v)" />
        </NFormItem>
      </template>
    </div>
    <div class="grid-2">
      <template v-for="p in PREDEFINED.slice(12, 14)" :key="p.key">
        <NFormItem :label="renderField(p).label">
          <NInput :value="renderField(p).value" @update:value="(v: string) => setValue(p.key, p.label, p.unit, v)" />
        </NFormItem>
      </template>
    </div>

    <div v-if="customChars.length" class="custom">
      <div class="custom-head">Пользовательские характеристики</div>
      <div v-for="c in customChars" :key="c.key" class="custom-row">
        <NInput v-model:value="c.label" placeholder="Название" />
        <NInput v-model:value="c.value" placeholder="Значение" />
        <NInput v-model:value="c.unit" placeholder="Ед." />
        <NButton size="small" tertiary type="error" @click="removeCustom(c)">×</NButton>
      </div>
    </div>

    <NButton dashed block @click="addCustom" style="margin-top: 16px;">+ Добавить характеристику</NButton>
  </section>
</template>

<style scoped>
.custom { margin-top: 20px; }
.custom-head { font-size: 12px; color: var(--color-text-secondary); text-transform: uppercase; letter-spacing: 0.04em; margin-bottom: 10px; }
.custom-row { display: grid; grid-template-columns: 1fr 1fr 80px 32px; gap: 8px; margin-bottom: 8px; align-items: center; }
</style>