import { Router } from "vue-router";
import { addToRouter } from "../../utils/routerAdd";
import FreeTableEdit from "./FreeTableEdit.vue";

let router:Router|undefined;

export function addTable(r:Router){
    addToRouter(r,routes);
    router = r;
}

const textSectionEditRouteName = 'freeTableEdit';
const routes = [
    {
        path:"/EditFreeTable/:id",
        component:FreeTableEdit,
        props:true,
        name:textSectionEditRouteName
    }
]

export function jumpToFreeTableEdit(id:number){
    router?.push({name:textSectionEditRouteName, params:{id:id}})
}