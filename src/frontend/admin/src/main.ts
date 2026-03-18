import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import Aura from '@primeuix/themes/aura'
import { definePreset } from '@primeuix/themes'
import ToastService from 'primevue/toastservice'
import ConfirmationService from 'primevue/confirmationservice'
import Tooltip from 'primevue/tooltip'

// Warm Stone palette — swaps Aura's cool-neutral surfaces for warm-tinted stone grays.
//
// Dark mode: Our palette reverses the scale so low indices = dark and high indices = light.
// Aura's semantic tokens reference HIGH indices for dark backgrounds (e.g. surface.900),
// so the dark colorScheme section remaps every semantic token to point at the correct
// inverted index from our palette.  The rule of thumb is:
//   Aura dark surface.N for bg  → our surface.(100 or 200)  [dark in our palette]
//   Aura dark surface.N for fg  → our surface.(900 or 950)  [light in our palette]
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
      dark: {
        surface: {
          0: '#0c0a09',
          50: '#1c1917',
          100: '#292524',
          200: '#44403c',
          300: '#57534e',
          400: '#78716c',
          500: '#a8a29e',
          600: '#d6d3d1',
          700: '#e7e5e4',
          800: '#f5f5f4',
          900: '#fafaf9',
          950: '#ffffff',
        },
        // Remap semantic tokens — Aura defaults reference high-numbered surfaces
        // for dark backgrounds, but our inverted palette has those as light values.
        text: {
          color: '{surface.900}',         // #fafaf9 — near-white text on dark bg
          hoverColor: '{surface.950}',    // #ffffff
          mutedColor: '{surface.600}',    // #d6d3d1 — readable muted text
          hoverMutedColor: '{surface.700}',
        },
        content: {
          background: '{surface.100}',      // #292524 — dark content area
          hoverBackground: '{surface.200}', // #44403c — hover
          borderColor: '{surface.200}',     // #44403c — subtle border
          color: '{surface.900}',           // #fafaf9 — text
          hoverColor: '{surface.950}',
        },
        overlay: {
          select: {
            background: '{surface.100}',
            borderColor: '{surface.200}',
            color: '{surface.900}',
          },
          popover: {
            background: '{surface.100}',
            borderColor: '{surface.200}',
            color: '{surface.900}',
          },
          modal: {
            background: '{surface.100}',
            borderColor: '{surface.200}',
            color: '{surface.900}',
          },
        },
        list: {
          option: {
            focusBackground: '{surface.200}',
            selectedBackground: 'color-mix(in srgb, {primary.color} 20%, transparent)',
            color: '{surface.900}',
            focusColor: '{surface.950}',
            selectedColor: '{primary.color}',
            selectedFocusBackground: 'color-mix(in srgb, {primary.color} 25%, transparent)',
            selectedFocusColor: '{primary.color}',
          },
          optionGroup: {
            background: 'transparent',
            color: '{surface.600}',
          },
        },
        navigation: {
          item: {
            focusBackground: '{surface.200}',
            activeBackground: '{surface.200}',
            color: '{surface.900}',
            focusColor: '{surface.950}',
            activeColor: '{surface.950}',
          },
          submenuLabel: {
            background: 'transparent',
            color: '{surface.600}',
          },
          submenuIcon: {
            color: '{surface.600}',
            focusColor: '{surface.700}',
            activeColor: '{surface.700}',
          },
        },
        formField: {
          background: '{surface.50}',          // #1c1917 — dark input bg
          disabledBackground: '{surface.200}',
          filledBackground: '{surface.100}',
          filledHoverBackground: '{surface.200}',
          filledFocusBackground: '{surface.100}',
          borderColor: '{surface.300}',        // #57534e — visible border
          hoverBorderColor: '{surface.400}',
          focusBorderColor: '{primary.color}',
          color: '{surface.900}',              // #fafaf9 — input text
          disabledColor: '{surface.500}',
          placeholderColor: '{surface.500}',   // #a8a29e — placeholder
          iconColor: '{surface.600}',
          shadow: 'none',
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
    options: {
      // Dark mode activates when <html class="dark"> is present.
      // The colorScheme store manages this class based on user preference.
      darkModeSelector: '.dark',
    },
  },
})
app.use(ToastService)
app.use(ConfirmationService)
app.directive('tooltip', Tooltip)

app.mount('#app')
