import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', redirect: '/objects' },
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
      meta: { public: true, layout: 'auth' },
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('@/views/RegisterView.vue'),
      meta: { public: true, layout: 'auth' },
    },
    {
      path: '/objects',
      name: 'objects',
      component: () => import('@/views/ObjectsListView.vue'),
    },
    {
      path: '/objects/new',
      name: 'object-new',
      component: () => import('@/views/ObjectEditView.vue'),
    },
    {
      path: '/objects/:id',
      name: 'object-edit',
      component: () => import('@/views/ObjectEditView.vue'),
      props: true,
    },
    {
      path: '/objects/:id/preview',
      name: 'object-preview',
      component: () => import('@/views/ObjectPreviewView.vue'),
      props: true,
    },
    {
      path: '/references',
      redirect: '/references/object-types',
    },
    {
      path: '/references/:type',
      name: 'references',
      component: () => import('@/views/ReferencesView.vue'),
      props: true,
    },
    {
      path: '/users',
      name: 'users',
      component: () => import('@/views/UsersView.vue'),
    },
    {
      path: '/settings',
      name: 'settings',
      component: () => import('@/views/SettingsView.vue'),
    },
    { path: '/403', name: 'error-403', component: () => import('@/views/Error403View.vue') },
    { path: '/404', name: 'error-404', component: () => import('@/views/Error404View.vue') },
    { path: '/500', name: 'error-500', component: () => import('@/views/Error500View.vue') },
    { path: '/:pathMatch(.*)*', name: 'not-found', component: () => import('@/views/Error404View.vue') },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (!to.meta.public && !auth.isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }
  if (to.meta.public && auth.isAuthenticated && (to.name === 'login' || to.name === 'register')) {
    return { name: 'objects' }
  }
})

export default router