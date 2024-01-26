import { Router } from "vue-router";
import { addToRouter } from "../../utils/routerAdd";
import editInfo from "./WikiItemEdit.vue"

export function addWiki(r:Router){
    addToRouter(r,routes);
}

const routes = [
    {
        path:"/w/Edit/:urlPathName",
        component:editInfo,
        props:true,
        name:"wikiEdit"
    }
]