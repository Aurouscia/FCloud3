import { Router } from "vue-router";
import { addToRouter } from "../../../utils/routerAdd";
import TextSectionEdit from "../TextSectionEdit.vue";
import { textSectionEditRouteName } from "./routesJump";

export function addTextSection(r:Router){
    addToRouter(r,routes);
}

const routes = [
    {
        path:"/EditTextSection/:id",
        component:TextSectionEdit,
        props:true,
        name:textSectionEditRouteName
    }
]