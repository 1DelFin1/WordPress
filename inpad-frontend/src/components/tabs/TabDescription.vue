<script setup lang="ts">
import { computed } from 'vue'
import { NInput, NFormItem } from 'naive-ui'
import type { ArchObject } from '@/api/types'

const props = defineProps<{ form: ArchObject }>()
const shortLen = computed(() => (props.form.shortDescription || '').length)
const fullLen = computed(() => (props.form.fullDescription || '').length)
</script>

<template>
  <section class="card">
    <h3 class="card-title">Текстовое описание</h3>
    <NFormItem label="Краткое описание *">
      <NInput v-model:value="form.shortDescription" type="textarea" :rows="3" :maxlength="500" />
    </NFormItem>
    <div class="count">{{ shortLen }} символов</div>

    <NFormItem label="Полное описание" style="margin-top: 16px;">
      <NInput v-model:value="form.fullDescription" type="textarea" :rows="10" :maxlength="2000" />
    </NFormItem>
    <div class="count">{{ fullLen }} символов из 2000</div>
  </section>
</template>

<style scoped>
.count { font-size: 12px; color: var(--color-text-secondary); margin-top: -8px; }
</style>