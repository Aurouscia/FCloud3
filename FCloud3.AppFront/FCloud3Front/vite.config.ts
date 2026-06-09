import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'
import { appVersionMark, ViteAppVersionConfig } from '@aurouscia/vite-app-version'
const versionConfig:ViteAppVersionConfig = {filePath: 'feVersion.txt'}

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, 'env', 'VITE_')

  return {
    plugins: [
      vue(),
      appVersionMark(versionConfig),
    ],
    define: {
      __VERSION_CONFIG__: `'${JSON.stringify(versionConfig)}'`
    },
    server: {
      port: 5173,
      proxy:{
        '/api':{
          target: env.VITE_ProxyTarget,
          changeOrigin: true
        },
        '/fickit':{
          target: env.VITE_ProxyTarget,
          changeOrigin: true
        }
      }
    },
    build: {
      outDir: "../../FCloud3.App/wwwroot",
      emptyOutDir: true,
      rollupOptions: {
        output: {
          manualChunks: (id) => {
            if (id.includes('@aurouscia'))
              return 'au'
            if (id.includes('@fickit'))
              return 'fickit'
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
    envDir: "env"
  }
})
