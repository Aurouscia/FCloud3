import { defineConfig } from 'vite'

export default defineConfig(({})=>{
  return{
    build: {
      lib: {
        formats: ['es'],
        entry: './lib/auTableConfig.ts',
        fileName: '[name]-[hash].entry',
      },
      emptyOutDir: true,
      outDir:'../../FCloud3Front/public/plugins/AuTableConfig',
    }
  }
})
