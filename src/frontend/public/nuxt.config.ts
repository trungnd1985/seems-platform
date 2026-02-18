export default defineNuxtConfig({
  compatibilityDate: '2025-01-01',
  devtools: { enabled: true },

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
  },

  site: {
    url: process.env.NUXT_PUBLIC_SITE_URL || 'http://localhost:3000',
  },

  css: ['~/assets/css/main.css'],

  routeRules: {
    '/': { isr: 60 },
    '/**': { isr: 60 },
  },
})
