import { Router } from "vue-router";
import { addToRouter } from "@/utils/routerAdd";
import WikiItemEdit from "../WikiItemEdit.vue"
import WikiInDirLocations from "../WikiInDirLocations.vue";

export function addWiki(r:Router){
    addToRouter(r,routes);
}

const routes = [
    {
        path:"/w/Edit/:urlPathName",
        component:WikiItemEdit,
        props:true,
        name:"wikiEdit"
    },
    {
        path:"/w/Locations/:urlPathName",
        component:WikiInDirLocations,
        props:true,
        name:"wikiLocations"
    }
]