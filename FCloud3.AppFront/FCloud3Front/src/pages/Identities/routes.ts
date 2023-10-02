import { Router } from "vue-router";
import { addToRouter } from "../../utils/routerAdd";
import login from "./login.vue"
import personal from "./personal.vue"

export function addIdentities(r:Router){
    addToRouter(r,routes);
}

const routes = [
    {
        path:"/User/Login",
        component:login
    },
    {
        path:"/User/Personal",
        component:personal
    }
]