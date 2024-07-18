import { Router } from "vue-router";
import { addToRouter } from "@/utils/routerAdd";
import Login from "../Login.vue"
import UserGroupIndex from "../UserGroupIndex.vue";
import UserList from "../UserList.vue";
import UserCenter from "../UserCenter.vue";
import Register from "../Register.vue";
import GlobalAuthGrants from "../GlobalAuthGrants.vue";
import UserCenterFromId from "../UserCenterFromId.vue";

export function addIdentities(r:Router){
    addToRouter(r,routes);
}

const routes = [
    {
        path:"/Login/:backAfterSuccess?",
        component:Login,
        props:true,
        name:'login'
    },
    {
        path:"/Register",
        component:Register,
        name:"register"
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
    },
    {
        path:"/uid/:uid",
        component:UserCenterFromId,
        props:true,
        name:'userCenterFromId'
    },
    {
        path:"/GlobalAuthGrants",
        component:GlobalAuthGrants,
        name:'gag'
    }
]