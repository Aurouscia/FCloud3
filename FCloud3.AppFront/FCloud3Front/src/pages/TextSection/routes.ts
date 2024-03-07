import { Router } from "vue-router";
import { addToRouter } from "../../utils/routerAdd";
import TextParaEditor from "./TextSectionEdit.vue"

let router:Router|undefined = undefined;

export function addTextSection(r:Router){
    addToRouter(r,routes);
    router = r;
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
    router?.push({name:textSectionEditRouteName, params:{id:id}})
}