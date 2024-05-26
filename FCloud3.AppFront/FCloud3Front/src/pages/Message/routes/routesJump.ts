import { useRouter } from "vue-router";

export function useMessageRoutesJump(){
    const router = useRouter();
    const jumpToNotifs = () => {
        router.push({name:"notifs"});
    }
    return { jumpToNotifs }
}