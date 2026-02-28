export default defineNuxtConfig({
  compatibilityDate: '2025-01-01',
  devtools: { enabled: false },

  experimental: {
    appManifest: false,
  },

  components: [{ path: '~/components', pathPrefix: false }],

  // SSR disabled in dev: avoids Nitro SSR worker OOM on first request.
  // Production builds always use SSR.
  ssr: process.env.NODE_ENV === 'production',

  modules: [
    '@nuxt/image',
    // SEO modules are only needed in production builds
    ...(process.env.NODE_ENV === 'production'
      ? ['@nuxtjs/sitemap', '@nuxtjs/robots', 'nuxt-schema-org']
      : []),
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
      // Proxy module static bundles served from the API's wwwroot
      '/modules': {
        target: 'http://localhost:5000/modules',
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

  devServer: {
    host: '0.0.0.0',
    port: 3000,
  },

  vite: {
    server: {
      watch: {
        ignored: ['**/node_modules/**', '**/.git/**'],
      },
      // Pre-warm pages so transforms happen at startup, not on first request
      warmup: {
        clientFiles: ['./pages/index.vue', './pages/[...slug].vue'],
        ssrFiles: ['./pages/index.vue', './pages/[...slug].vue'],
      },
    },
    build: {
      sourcemap: false,
      rollupOptions: {
        // Limit parallel file operations to reduce worker thread pressure
        maxParallelFileOps: 3,
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
