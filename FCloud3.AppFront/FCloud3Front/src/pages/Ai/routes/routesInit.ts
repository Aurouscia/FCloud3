import { Router } from "vue-router";
import { addToRouter } from "@/utils/routerAdd";
import AiChatPage from "../AiChatPage.vue";
import AiInstanceListPage from "../AiInstanceListPage.vue";
import AiInstanceEditPage from "../AiInstanceEditPage.vue";

export function addAi(r: Router) {
    addToRouter(r, routes);
}

const routes = [
    {
        path: "/AiChat/:groupId(\\d+)?",
        component: AiChatPage,
        props: true,
        name: "aiChat"
    },
    {
        path: "/AiInstance/:groupId(\\d+)",
        component: AiInstanceListPage,
        props: true,
        name: "aiInstanceList"
    },
    {
        path: "/AiInstance/Edit/:instanceId(\\d+)?",
        component: AiInstanceEditPage,
        props: true,
        name: "aiInstanceEdit"
    }
];
