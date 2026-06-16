export function useAiRoutesJump() {
    const jumpToAiChatRoute = (groupId?: number) => {
        return { name: "aiChat", params: { groupId: groupId?.toString() } };
    };
    return { jumpToAiChatRoute };
}
