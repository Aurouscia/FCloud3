import { Router } from "vue-router";
import { addToRouter } from "@/utils/routerAdd";
import WikiItemEdit from "../WikiItemEdit.vue"
import WikiInDirLocations from "../WikiInDirLocations.vue";
import WikiItemContentEdit from "../WikiItemContentEdit.vue";
import WikiContentSearch from "../WikiContentSearch.vue";
import MyWikisOverall from "../MyWikisOverall.vue";

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
        path:"/w/EditContent/:urlPathName",
        component:WikiItemContentEdit,
        props:true,
        name:"wikiContentEdit"
    },
    {
        path:"/w/Locations/:urlPathName",
        component:WikiInDirLocations,
        props:true,
        name:"wikiLocations"
    },
    {
        path:"/WikiContentSearch",
        component:WikiContentSearch,
        name:"wikiContentSearch"
    },
    {
        path:"/MyWikis/:uid?",
        component:MyWikisOverall,
        props:true,
        name:"myWikisOverall"
    }
]