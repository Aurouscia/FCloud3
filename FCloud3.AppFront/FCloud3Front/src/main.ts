import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import { createRouter, createWebHashHistory } from 'vue-router';
import { addIdentities } from './pages/Identities/routes'
import NotFound from './pages/notFound.vue';
import { addHomePage } from './pages/Home/routes';

const routes = [{
        path: '/:pathMatch(.*)*',
        component: NotFound 
    }]

const router = createRouter({
    history: createWebHashHistory(),
    routes:routes
})
addIdentities(router)
addHomePage(router)


createApp(App).use(router).mount('#app')