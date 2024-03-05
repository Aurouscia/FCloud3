import { Router } from "vue-router";
import { addToRouter } from "../../utils/routerAdd";
import WikiTemplateList from "./WikiTemplateList.vue"
import WikiTemplateEditor from "./WikiTemplateEditor.vue";

let router:Router|undefined = undefined;

export function addWikiParsing(r:Router){
    addToRouter(r,routes);
    router = r;
}

const routes = [
    {
        path:"/templates",
        component:WikiTemplateList,
        name:"wikiTemplateList"
    },{
        path:"/templates/:id",
        component:WikiTemplateEditor,
        props:true,
        name:"wikiTemplateEditor"
    }
]

export function jumpToWikiTemplateList(){
    if(router){
        router.push({name:"wikiTemplateList"});
    }
}
export function jumpToWikiTemplateEditor(id:number){
    if(router){
        router.push({name:"wikiTemplateEditor",params:{id}});
    }
}