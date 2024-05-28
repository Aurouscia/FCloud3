import { Router } from "vue-router";
import { addToRouter } from "@/utils/routerAdd";
import FileDirIndex from '../FileDirIndex.vue'
import Material from "../Material.vue";
import FileDirFromId from "../FileDirFromId.vue";

export function addFiles(r:Router){
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