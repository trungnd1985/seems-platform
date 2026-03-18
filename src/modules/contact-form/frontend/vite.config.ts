import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import cssInjectedByJs from 'vite-plugin-css-injected-by-js'
import { fileURLToPath, URL } from 'node:url'

const vueBridgePath = fileURLToPath(new URL('./vue-bridge.js', import.meta.url))

export default defineConfig({
  plugins: [vue(), cssInjectedByJs()],
  resolve: {
    alias: {
      // Redirect bare 'vue' imports to the host app's runtime via the bridge,
      // so this bundle never ships its own Vue copy.
      vue: vueBridgePath,
    },
  },
  build: {
    outDir: '../../../backend/Seems.Api/wwwroot/modules/contact-form',
    emptyOutDir: true,
    lib: {
      entry: 'index.ts',
      formats: ['es'],
      fileName: 'component',
    },
  },
})
