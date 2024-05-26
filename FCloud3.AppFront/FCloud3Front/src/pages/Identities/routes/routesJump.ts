import { useRouter } from "vue-router"

export function useIdentityRoutesJump(){
    const router = useRouter();
    const jumpToUserCenter = (username:string) => {
        router.push({name:'userCenter', params:{username}})
    }
    const jumpToLogin = (backAfterSuccess:boolean = true) => {
        router.push({name:'login', params:{backAfterSuccess: backAfterSuccess?'back':undefined}})
    }
    const jumpToRegister = () => {
        router.push({name:'register'})
    }
    return { jumpToLogin, jumpToUserCenter, jumpToRegister }
}