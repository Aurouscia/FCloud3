import { Router } from "vue-router";
import { addToRouter } from "@/utils/routerAdd";
import AiChatPage from "../AiChatPage.vue";

export function addAi(r: Router) {
    addToRouter(r, routes);
}

const routes = [
    {
        path: "/AiChat/:groupId(\\d+)?",
        component: AiChatPage,
        props: true,
        name: "aiChat"
    }
];
