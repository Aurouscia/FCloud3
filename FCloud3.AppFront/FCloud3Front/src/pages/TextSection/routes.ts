import { Router } from "vue-router";
import { addToRouter } from "../../utils/routerAdd";
import TextParaEditor from "./TextSectionEditor.vue"
import { router } from "../../main";

export function addTextSection(r:Router){
    addToRouter(r,routes);
}

const textSectionEditRouteName = 'textSectionEdit';
const routes = [
    {
        path:"/EditTextSection/:id",
        component:TextParaEditor,
        props:true,
        name:textSectionEditRouteName
    }
]

export function jumpToTextSectionEdit(id:number){
    router.push({name:textSectionEditRouteName, params:{id:id}})
}