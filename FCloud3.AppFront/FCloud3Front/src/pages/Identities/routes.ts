import { Router } from "vue-router";
import { addToRouter } from "../../utils/routerAdd";
import Login from "./Login.vue"
import UserGroupIndex from "./UserGroupIndex.vue";
import UserList from "./UserList.vue";
import UserCenter from "./UserCenter.vue";

let router:Router;
export function addIdentities(r:Router){
    addToRouter(r,routes);
    router = r;
}

const routes = [
    {
        path:"/Login/:backAfterSuccess?",
        component:Login,
        props:true,
        name:'login'
    },
    {
        path:"/UserGroup/:id(\\d+)?",
        component:UserGroupIndex,
        props:true,
        name:'userGroup'
    },
    {
        path:"/UserList",
        component:UserList
    },
    {
        path:"/u/:username?",
        component:UserCenter,
        props:true,
        name:'userCenter'
    }
]

export function jumpToUserCenter(username:string){
    router.push({name:'userCenter', params:{username}})
}
export function jumpToLogin(backAfterSuccess:boolean = true){
    router.push({name:'login', params:{backAfterSuccess: backAfterSuccess?'back':undefined}})
}