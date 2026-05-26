<script setup lang="ts">
</script>

<template>
  <div class="auth">
    <aside class="auth-left">
      <div class="logo-card">
        <div class="logo-mark">ИНПАД</div>
        <div class="logo-sub">Архитектурное бюро</div>
      </div>
    </aside>
    <section class="auth-right">
      <slot />
    </section>
  </div>
</template>

<style scoped>
.auth {
  display: grid;
  grid-template-columns: 1fr 1fr;
  min-height: 100vh;
}
.auth-left {
  background: #A8A892;
  display: flex;
  align-items: center;
  justify-content: center;
}
.logo-card {
  background: #fff;
  width: 280px;
  height: 280px;
  border-radius: 12px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  box-shadow: 0 8px 32px rgba(0,0,0,0.08);
}
.logo-mark {
  font-size: 36px;
  font-weight: 700;
  letter-spacing: 0.1em;
  color: #1A1A1A;
}
.logo-sub {
  margin-top: 8px;
  font-size: 13px;
  color: var(--color-text-secondary);
  letter-spacing: 0.04em;
}
.auth-right {
  background: #FAFAF8;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px;
}

@media (max-width: 900px) {
  .auth { grid-template-columns: 1fr; }
  .auth-left { display: none; }
}
</style>