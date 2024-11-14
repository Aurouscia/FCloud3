import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue()],
  server:{
    port:5173
  },
  build:{
    outDir:"../../FCloud3.App/wwwroot",
    emptyOutDir:true,
    rollupOptions:{
      output:{
        manualChunks:{
          'auTableEditor':['@aurouscia/au-table-editor'],
          'libs':[
            'lodash','axios','md5','pinia','vue-router',
            '@aurouscia/keyboard-shortcut'],
          'vue':['vue']
        }
      }
    }
  },
  resolve:{
    alias:{
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
