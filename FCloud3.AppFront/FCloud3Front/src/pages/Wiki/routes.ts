import { Router } from "vue-router";
import { addToRouter } from "../../utils/routerAdd";
import editInfo from "./editWikiInfo.vue"

export function addWiki(r:Router){
    addToRouter(r,routes);
}

const routes = [
    {
        path:"/Wiki/EditInfo",
        component:editInfo
    }
]