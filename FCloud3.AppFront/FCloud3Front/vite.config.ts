import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'
import { appVersionMark, ViteAppVersionConfig } from '@aurouscia/vite-app-version'
const versionConfig:ViteAppVersionConfig = {filePath: 'feVersion.txt'}

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    appVersionMark(versionConfig),
  ],
  define: {
    __VERSION_FILE__: `'${JSON.stringify(versionConfig)}'`
  },
  server: {
    port: 5173
  },
  build: {
    outDir: "../../FCloud3.App/wwwroot",
    emptyOutDir: true,
    rollupOptions: {
      output: {
        manualChunks: (id) => {
          if (id.includes('@aurouscia'))
            return 'au'
          if (id.includes('node_modules'))
            return 'libs'
        }
      }
    }
  },
  resolve: {
    alias: {
      '@': resolve(__dirname, './src'),
      '~': resolve('')
    }
  },
  css: {
    preprocessorOptions: {
      scss: {
        api: 'modern-compiler'
      },
    },
  },
  envDir: "dev/env"
})
