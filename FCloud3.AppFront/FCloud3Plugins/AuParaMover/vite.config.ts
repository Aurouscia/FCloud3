import { defineConfig } from 'vite'

export default defineConfig((_)=>{
  return{
    server:{
      port: 5176
    },
    build: {
      lib: {
        formats: ['es'],
        entry: './lib/auParaMover.ts',
        fileName: '[name]-[hash].entry',
      },
      emptyOutDir: true,
      outDir:'../../FCloud3Front/public/plugins/AuParaMover',
    }
  }
})
