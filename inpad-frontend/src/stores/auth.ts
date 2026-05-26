import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { api } from '@/api/client'
import type { User } from '@/api/types'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('inpad_token'))
  const userRaw = localStorage.getItem('inpad_user')
  const user = ref<User | null>(userRaw ? (JSON.parse(userRaw) as User) : null)

  const isAuthenticated = computed(() => !!token.value)

  function setAuth(t: string, u: User) {
    token.value = t
    user.value = u
    localStorage.setItem('inpad_token', t)
    localStorage.setItem('inpad_user', JSON.stringify(u))
  }

  async function login(email: string, password: string) {
    const res = await api.login(email, password)
    if (res?.token && res?.user) setAuth(res.token, res.user)
    return res
  }

  async function register(payload: Record<string, unknown>) {
    return await api.register(payload)
  }

  function logout() {
    token.value = null
    user.value = null
    localStorage.removeItem('inpad_token')
    localStorage.removeItem('inpad_user')
  }

  return { token, user, isAuthenticated, login, register, logout, setAuth }
})