import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/auth/LoginView.vue'),
      meta: { public: true },
    },
    {
      path: '/',
      component: () => import('@/components/layout/AdminShell.vue'),
      children: [
        {
          path: '',
          name: 'dashboard',
          component: () => import('@/views/dashboard/DashboardView.vue'),
        },
        {
          path: 'pages',
          name: 'pages',
          component: () => import('@/views/pages/PageListView.vue'),
        },
        {
          path: 'content-types',
          name: 'content-types',
          component: () => import('@/views/content/ContentTypeListView.vue'),
        },
        {
          path: 'content-items',
          name: 'content-items',
          component: () => import('@/views/content/ContentItemListView.vue'),
        },
        {
          path: 'templates',
          name: 'templates',
          component: () => import('@/views/templates/TemplateListView.vue'),
        },
        {
          path: 'themes',
          name: 'themes',
          component: () => import('@/views/themes/ThemeManagerView.vue'),
        },
        {
          path: 'media',
          name: 'media',
          component: () => import('@/views/media/MediaLibraryView.vue'),
        },
        {
          path: 'modules',
          name: 'modules',
          component: () => import('@/views/modules/ModuleListView.vue'),
        },
        {
          path: 'roles',
          name: 'roles',
          component: () => import('@/views/roles/RoleListView.vue'),
          meta: { roles: ['Admin'] },
        },
        {
          path: 'users',
          name: 'users',
          component: () => import('@/views/users/UserListView.vue'),
          meta: { roles: ['Admin'] },
        },
        {
          path: 'settings',
          name: 'settings',
          component: () => import('@/views/settings/SiteSettingsView.vue'),
        },
      ],
    },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()

  if (!to.meta.public && !auth.isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  const requiredRoles = to.meta.roles as string[] | undefined
  if (requiredRoles && auth.user && !requiredRoles.some((r) => auth.user!.roles.includes(r))) {
    return { name: 'dashboard' }
  }
})

export default router
