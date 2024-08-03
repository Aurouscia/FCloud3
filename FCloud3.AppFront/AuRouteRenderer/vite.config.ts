import { defineConfig } from 'vite'

export default defineConfig({
  server:{
    port:5174
  },
  build: {
    lib: {
      formats:['es'],
      entry: './lib/auRouteRenderer.ts',
      fileName: 'dist'
    },
    emptyOutDir: true,
    outDir:'../FCloud3Front/public/plugins/AuRouteRenderer',
    copyPublicDir: false
  }
})
