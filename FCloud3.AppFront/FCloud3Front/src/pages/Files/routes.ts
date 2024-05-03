import { Router } from "vue-router";
import { addToRouter } from "../../utils/routerAdd";
import FileDirIndex from './FileDirIndex.vue'
import Material from "./Material.vue";
import FileDirFromId from "./FileDirFromId.vue";

let router:Router;
export function addFiles(r:Router){
    router = r;
    addToRouter(r,routes);
}

const routes = [
    {
        name: 'files',
        path: '/d/:path(.*)*',
        component: FileDirIndex,
        props:true
    },
    {
        name: 'filesFromId',
        path: '/did/:id',
        component: FileDirFromId,
        props:true
    },
    {
        name: 'materials',
        path: '/materials',
        component: Material,
    }
]

export function jumpToDirFromId(id:number){
    router.push({name:'filesFromId', params:{id}})
}