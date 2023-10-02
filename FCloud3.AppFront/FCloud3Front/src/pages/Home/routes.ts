import { Router } from "vue-router";
import { addToRouter } from "../../utils/routerAdd";
import home from "./home.vue"

export function addHomePage(r:Router){
    addToRouter(r,routes);
}

const routes = [
    {
        path:"/HomePage",
        component:home
    },
]