import { Router } from "vue-router";
import { addToRouter } from "@/utils/routerAdd"
import DiffContentHistory from "../DiffContentHistory.vue";

export function addDiff(r:Router){
    addToRouter(r,routes);
}

const routes = [
    {
        path:"/History/:type/:objId",
        component: DiffContentHistory,
        props: true,
        name: 'diffContentHistory'
    }
]