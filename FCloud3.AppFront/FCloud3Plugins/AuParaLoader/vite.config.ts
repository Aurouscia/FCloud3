import { defineConfig } from 'vite'

export default defineConfig((_)=>{
  return{
    server:{
      port: 5173
    },
    build: {
      lib: {
        formats: ['es'],
        entry: './lib/auParaLoader.ts',
        fileName: '[name]-[hash].entry',
      },
      emptyOutDir: true,
      outDir:'../../FCloud3Front/public/plugins/AuParaLoader',
    }
  }
})
