import { Router } from "vue-router";
import { addToRouter } from "../../utils/routerAdd";
import WikiItemEdit from "./WikiItemEdit.vue"

let router:Router|undefined
export function addWiki(r:Router){
    router = r;
    addToRouter(r,routes);
}

const routes = [
    {
        path:"/w/Edit/:urlPathName",
        component:WikiItemEdit,
        props:true,
        name:"wikiEdit"
    }
]

export function jumpToWikiEdit(urlPathName:string){
    router?.push({name:"wikiEdit", params:{urlPathName}})
}