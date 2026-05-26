<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { NConfigProvider, NMessageProvider, NDialogProvider, ruRU, dateRuRU } from 'naive-ui'
import type { GlobalThemeOverrides } from 'naive-ui'
import AppLayout from '@/layouts/AppLayout.vue'
import AuthLayout from '@/layouts/AuthLayout.vue'

const route = useRoute()
const layout = computed(() => (route.meta.layout === 'auth' ? AuthLayout : AppLayout))

const themeOverrides: GlobalThemeOverrides = {
  common: {
    primaryColor: '#8C8B72',
    primaryColorHover: '#767566',
    primaryColorPressed: '#5E5D54',
    primaryColorSuppl: '#8C8B72',
    borderRadius: '6px',
    fontFamily:
      '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Oxygen, Ubuntu, "Helvetica Neue", sans-serif',
  },
  Button: {
    colorPrimary: '#8C8B72',
    colorHoverPrimary: '#767566',
    colorPressedPrimary: '#5E5D54',
    colorFocusPrimary: '#8C8B72',
    textColorPrimary: '#fff',
    borderPrimary: '1px solid #8C8B72',
    borderHoverPrimary: '1px solid #767566',
    borderPressedPrimary: '1px solid #5E5D54',
    borderFocusPrimary: '1px solid #8C8B72',
  },
}
</script>

<template>
  <NConfigProvider :theme-overrides="themeOverrides" :locale="ruRU" :date-locale="dateRuRU">
    <NMessageProvider>
      <NDialogProvider>
        <component :is="layout">
          <RouterView />
        </component>
      </NDialogProvider>
    </NMessageProvider>
  </NConfigProvider>
</template>