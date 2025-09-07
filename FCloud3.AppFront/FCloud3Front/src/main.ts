import { createApp } from 'vue'
import './style.scss'
import '@aurouscia/au-table-editor/style.css'
import App from './App.vue'
import { createPinia } from 'pinia';
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import { createRouter, createWebHashHistory } from 'vue-router';
import { recoverTitle as initTitle } from './utils/titleSetter';
import { configureRoute } from './routeConfigure';

initTitle()

const router = createRouter({
    history: createWebHashHistory(),
    routes:[]
})
configureRoute(router)

const pinia = createPinia().use(piniaPluginPersistedstate);

createApp(App).use(router).use(pinia).mount('#app')