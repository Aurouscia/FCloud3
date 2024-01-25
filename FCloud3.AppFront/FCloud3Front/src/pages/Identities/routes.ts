import { Router } from "vue-router";
import { addToRouter } from "../../utils/routerAdd";
import Login from "../../components/Login.vue"
import UserGroupIndex from "./UserGroupIndex.vue";
import UserCenter from "./UserCenter.vue";

export function addIdentities(r:Router){
    addToRouter(r,routes);
}

const routes = [
    {
        path:"/Login",
        component:Login
    },
    {
        path:"/UserGroup/:id(\\d+)?",
        component:UserGroupIndex,
        props:true,
        name:'userGroup'
    },
    {
        path:"/u/:username?",
        component:UserCenter,
        props:true
    }
]