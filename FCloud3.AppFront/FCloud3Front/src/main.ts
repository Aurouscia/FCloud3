import { createApp } from 'vue'
import './style.css'
import './wikiViewStyles.css'
import App from './App.vue'
import { createRouter, createWebHashHistory } from 'vue-router';
import { addIdentities } from './pages/Identities/routes'
import NotFound from './pages/NotFound.vue';
import { addHomePage } from './pages/Home/routes';
import { addWiki } from './pages/Wiki/routes';
import { addTextSection } from './pages/TextSection/routes';
import { addFiles } from './pages/Files/routes';
import { addTable } from './pages/Table/routes';

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
addWiki(router)
addTextSection(router)
addTable(router)
addFiles(router)

createApp(App).use(router).mount('#app')