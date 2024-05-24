import { createApp } from 'vue'
import './style.scss'
import App from './App.vue'
import { createPinia } from 'pinia';
import { createRouter, createWebHashHistory } from 'vue-router';
import { recoverTitle as initTitle } from './utils/titleSetter';
import { configureRoute } from './routeConfigure';

initTitle()

const router = createRouter({
    history: createWebHashHistory(),
    routes:[]
})
configureRoute(router)

const pinia = createPinia();

createApp(App).use(router).use(pinia).mount('#app')