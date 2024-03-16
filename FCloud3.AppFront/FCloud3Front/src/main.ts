import { createApp } from 'vue'
import './style.css'
import './wikiViewStyles.scss'
import App from './App.vue'
import { createRouter, createWebHashHistory } from 'vue-router';
import { addIdentities } from './pages/Identities/routes'
import NotFound from './pages/NotFoundPage.vue';
import { addHomePage } from './pages/Home/routes';
import { addWiki } from './pages/Wiki/routes';
import { addTextSection } from './pages/TextSection/routes';
import { addFiles } from './pages/Files/routes';
import { addTable } from './pages/Table/routes';
import { addWikiParsing } from './pages/WikiParsing/routes';

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
addWikiParsing(router)
addTextSection(router)
addTable(router)
addFiles(router)

createApp(App).use(router).mount('#app')