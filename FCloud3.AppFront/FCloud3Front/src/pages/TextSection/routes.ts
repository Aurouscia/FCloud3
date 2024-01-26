import { Router } from "vue-router";
import { addToRouter } from "../../utils/routerAdd";
import textParaEditor from "./TextSectionEditor.vue"

export function addTextSection(r:Router){
    addToRouter(r,routes);
}

const routes = [
    {
        path:"/EditTextSection/:id",
        component:textParaEditor,
        props:true
    }
]