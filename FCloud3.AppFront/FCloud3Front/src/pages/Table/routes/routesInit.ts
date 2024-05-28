import { Router } from "vue-router";
import { addToRouter } from "@/utils/routerAdd";
import FreeTableEdit from "../FreeTableEdit.vue";
import { tableEditRouteName } from "./routesJump";

export function addTable(r:Router){
    addToRouter(r,routes);
}

const routes = [
    {
        path:"/EditFreeTable/:id",
        component:FreeTableEdit,
        props:true,
        name:tableEditRouteName
    }
]