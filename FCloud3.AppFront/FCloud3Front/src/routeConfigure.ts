import { addIdentities } from './pages/Identities/routes/routesInit'
import { addHomePage } from './pages/Home/routes/routesInit';
import { addWiki } from './pages/Wiki/routes/routesInit';
import { addTextSection } from './pages/TextSection/routes/routesInit';
import { addFiles } from './pages/Files/routes/routesInit';
import { addTable } from './pages/Table/routes/routesInit';
import { addWikiParsing } from './pages/WikiParsing/routes/routesInit';
import { addDiff } from './pages/Diff/routes/routesInit';
import { addMessages } from './pages/Message/routes/routesInit';
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