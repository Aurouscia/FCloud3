import { Router } from "vue-router";
import { addToRouter } from "../../utils/routerAdd";
import fileDir from './fileDir.vue'

export function addFiles(r:Router){
    addToRouter(r,routes);
}

const routes = [
    {
        name: 'files',
        path: '/files/:path(.*)*',
        component: fileDir,
        props:true
    }
]