export default defineNuxtConfig({
  compatibilityDate: '2025-01-01',
  devtools: { enabled: true },

  components: [{ path: '~/components', pathPrefix: false }],

  modules: [
    '@nuxt/image',
    '@nuxtjs/sitemap',
    '@nuxtjs/robots',
    'nuxt-schema-org',
  ],  

  runtimeConfig: {
    apiBase: 'http://localhost:5000/api',
    public: {
      apiBase: '/api',
      siteUrl: 'http://localhost:3000',
    },
  },

  nitro: {
    devProxy: {
      '/api': {
        target: 'http://localhost:5000/api',
        changeOrigin: true,
      },
    },
    watchOptions: {
      ignored: ['**/node_modules/**', '**/.git/**', '**/.nuxt/**'],
    },
  },

  site: {
    url: process.env.NUXT_PUBLIC_SITE_URL,
    name: 'SEEMS Platform',
  },

  vite: {
    server: {
      watch: {
        ignored: ['**/node_modules/**', '**/.git/**'],
      },
    },
  },

  css: ['~/assets/css/main.css'],

  $production: {
    routeRules: {
      '/': { isr: 60 },
      '/**': { isr: 60 },
    },
  },
})
