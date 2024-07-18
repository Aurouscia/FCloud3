import { Router } from "vue-router";
import { addToRouter } from "@/utils/routerAdd";
import WikiTemplateList from "../WikiTemplateList.vue"
import WikiTemplateEditor from "../WikiTemplateEditor.vue";
import ViewWiki from "../ViewWiki.vue";
import ViewWikiFromId from "../ViewWikiFromId.vue";

export function addWikiParsing(r:Router){
    addToRouter(r,routes);
}

const routes = [
    {
        path:"/w/:wikiPathName",
        component: ViewWiki,
        props:true,
        name:"viewWiki",
    },
    {
        path:"/wFromId/:wikiId",
        component: ViewWikiFromId,
        props:true,
        name:"viewWikiFromId",
    },
    {
        path:"/w/:wikiPathName/:viewCmt",
        component: ViewWiki,
        props:true,
        name:"viewWikiCmt",
    },
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