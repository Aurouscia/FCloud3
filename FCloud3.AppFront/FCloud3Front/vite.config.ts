import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue()],
  server:{
    port:5173
  },
  build:{
    outDir:"../../FCloud3.App/wwwroot",
    emptyOutDir:true
  }
})
