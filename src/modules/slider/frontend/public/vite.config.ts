import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import cssInjectedByJs from 'vite-plugin-css-injected-by-js'
import { fileURLToPath, URL } from 'node:url'

// resolve.alias is applied by Vite's core BEFORE any plugin resolveId hooks,
// so this is the reliable way to redirect `import { ... } from 'vue'` to our
// bridge file that reads every export from window.__SEEMS_VUE__ at runtime.
const vueBridgePath = fileURLToPath(new URL('./vue-bridge.js', import.meta.url))

export default defineConfig({
  plugins: [vue(), cssInjectedByJs()],
  resolve: {
    alias: {
      vue: vueBridgePath,
    },
  },
  build: {
    outDir: '../../../../backend/Seems.Api/wwwroot/modules/slider',
    emptyOutDir: true,
    lib: {
      entry: 'index.ts',
      formats: ['es'],
      fileName: 'component',
    },
  },
})
