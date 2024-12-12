import { defineConfig } from 'vite'

const publicDirBuild = 'public/publicBuild'
const publicDirDev = 'public/publicDev'

export default defineConfig(({command})=>{
  const publicDir = command === 'build' ? publicDirBuild : publicDirDev
  return{
    build: {
      lib: {
        entry: './lib/main.ts',
        name: 'Counter',
        fileName: 'counter',
      },
    },
    publicDir
  }
})
