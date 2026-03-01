import { Router } from "vue-router";
import { addToRouter } from "@/utils/routerAdd"
import Currency from "../Currency.vue";

export function addFicKit(r:Router){
    addToRouter(r,routes);
}

const routes = [
    {
        path:"/FicKit/Currency",
        component: Currency,
        name: 'fickit-currency'
    }
]