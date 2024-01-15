import { Router } from "vue-router";
import { addToRouter } from "../../utils/routerAdd";
import Login from "./login.vue"
import Personal from "./personal.vue"
import User from "./user.vue";

export function addIdentities(r:Router){
    addToRouter(r,routes);
}

const routes = [
    {
        path:"/User/Login",
        component:Login
    },
    {
        path:"/User/Personal",
        component:Personal
    },
    {
        path:"/u/:username?",
        component:User,
        props:true
    }
]