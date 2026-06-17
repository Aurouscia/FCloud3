import { useRouter } from "vue-router"

export function useAiRoutesJump() {
    const router = useRouter();
    const jumpToAiChatRoute = (aiInstanceConfigId?: number) => {
        return { name: "aiChat", params: { aiInstanceConfigId: aiInstanceConfigId?.toString() } };
    };
    const jumpToAiChat = (aiInstanceConfigId?: number) => {
        router.push(jumpToAiChatRoute(aiInstanceConfigId));
    };
    const jumpToAiInstanceListRoute = (groupId: number) => {
        return { name: "aiInstanceList", params: { groupId: groupId.toString() } };
    };
    const jumpToAiInstanceList = (groupId: number) => {
        router.push(jumpToAiInstanceListRoute(groupId));
    };
    const jumpToAiInstanceEditRoute = (instanceId?: number, groupId?: number) => {
        return {
            name: "aiInstanceEdit",
            params: { instanceId: instanceId?.toString() },
            query: groupId !== undefined ? { groupId: groupId.toString() } : undefined
        };
    };
    const jumpToAiInstanceEdit = (instanceId?: number, groupId?: number) => {
        router.push(jumpToAiInstanceEditRoute(instanceId, groupId));
    };
    return {
        jumpToAiChatRoute,
        jumpToAiChat,
        jumpToAiInstanceListRoute,
        jumpToAiInstanceList,
        jumpToAiInstanceEditRoute,
        jumpToAiInstanceEdit
    };
}
