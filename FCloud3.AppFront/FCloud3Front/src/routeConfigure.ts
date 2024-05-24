import { addIdentities } from './pages/Identities/routes'
import { addHomePage } from './pages/Home/routes';
import { addWiki } from './pages/Wiki/routes';
import { addTextSection } from './pages/TextSection/routes';
import { addFiles } from './pages/Files/routes';
import { addTable } from './pages/Table/routes';
import { addWikiParsing } from './pages/WikiParsing/routes';
import { addDiff } from './pages/Diff/routes';
import { addMessages } from './pages/Message/routes';
import NotFound from './pages/NotFoundPage.vue';
import { Router } from 'vue-router';

export function configureRoute(router:Router){
    router.addRoute({
        path: '/:pathMatch(.*)*',
        component: NotFound 
    })
    addIdentities(router)
    addHomePage(router)
    addWiki(router)
    addWikiParsing(router)
    addTextSection(router)
    addTable(router)
    addFiles(router)
    addDiff(router)
    addMessages(router)
}