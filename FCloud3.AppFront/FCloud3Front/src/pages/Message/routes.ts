import { Router } from "vue-router";
import Notifications from "./Notifications.vue";
import { addToRouter } from "../../utils/routerAdd";

let router:Router|undefined = undefined;

export function addMessages(r:Router){
    addToRouter(r,routes);
    router = r;
}

const routes = [
    {
        path:"/notifications",
        component: Notifications,
        name:"notifs"
    }
]

export function jumpToNotifs(){
    router?.push({name:"notifs"});
}