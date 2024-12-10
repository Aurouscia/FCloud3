import { defineConfig } from 'vite'

const publicDirBuild = 'public/publicBuild'
const publicDirDev = 'public/publicDev'

export default defineConfig(({command})=>{
  const publicDir = command === 'build' ? publicDirBuild : publicDirDev
  return{
    server:{
      port:5174
    },
    build: {
      lib: {
        formats:['es'],
        entry: './lib/auRouteRenderer.ts',
        fileName: '[name]-[hash].entry'
      },
      emptyOutDir: true,
      outDir:'../../FCloud3Front/public/plugins/AuRouteRenderer',
    },
    publicDir
  }
})
