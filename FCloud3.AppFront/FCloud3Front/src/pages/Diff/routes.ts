import { Router } from "vue-router";
import { addToRouter } from "../../utils/routerAdd";
import DiffContentHistory from "./DiffContentHistory.vue";
import { DiffContentType, diffContentTypeToStr } from "../../models/diff/DiffContentType";

let router:Router;
export function addDiff(r:Router){
    addToRouter(r,routes);
    router = r;
}

const routes = [
    {
        path:"/History/:type/:objId",
        component: DiffContentHistory,
        props: true,
        name: 'diffContentHistory'
    }
]

export function jumpToDiffContentHistory(type:DiffContentType, objId:number){
    router.push({name:'diffContentHistory', params:{type:diffContentTypeToStr(type), objId:objId}})
}