import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import Aura from '@primeuix/themes/aura'
import { definePreset } from '@primeuix/themes'
import ToastService from 'primevue/toastservice'
import ConfirmationService from 'primevue/confirmationservice'
import Tooltip from 'primevue/tooltip'

// Warm Stone palette — swaps Aura's cool-neutral surfaces for warm-tinted stone grays.
// All var(--p-surface-*) tokens across the entire UI resolve to these values automatically.
const WarmStone = definePreset(Aura, {
  semantic: {
    colorScheme: {
      light: {
        surface: {
          0: '#ffffff',
          50: '#fafaf9',
          100: '#f5f5f4',
          200: '#e7e5e4',
          300: '#d6d3d1',
          400: '#a8a29e',
          500: '#78716c',
          600: '#57534e',
          700: '#44403c',
          800: '#292524',
          900: '#1c1917',
          950: '#0c0a09',
        },
      },
    },
  },
})

import App from './App.vue'
import router from './router'

import 'primeicons/primeicons.css'
import './assets/css/main.css'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(PrimeVue, {
  theme: {
    preset: WarmStone,
    // Disable auto dark-mode detection — the admin is a light-only UI.
    // PrimeVue defaults to "system" which switches all component surfaces
    // to dark values via @media (prefers-color-scheme: dark), creating
    // a jarring mix of dark components on a light custom layout.
    options: {
      darkModeSelector: 'none',
    },
  },
})
app.use(ToastService)
app.use(ConfirmationService)
app.directive('tooltip', Tooltip)

app.mount('#app')
