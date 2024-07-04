import { Router } from "vue-router";
import { addToRouter } from "@/utils/routerAdd";
import HomePage from "../HomePage.vue"
import About from "../About.vue"
import GrammarHelp from "../GrammarHelp.vue";

export function addHomePage(r:Router){
    addToRouter(r,routes);
}

const routes = [
    {
        path:"/",
        component:HomePage
    },
    {
        path:"/HomePage",
        component:HomePage
    },
    {
        path:"/About",
        component:About
    },
    {
        path:"/GrammarHelp",
        component:GrammarHelp
    }
]