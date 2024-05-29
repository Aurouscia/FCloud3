import { useRouter } from "vue-router"

export function useIdentityRoutesJump(){
    const router = useRouter();
    const jumpToUserCenter = (username:string) => {
        router.push({name:'userCenter', params:{username}})
    }
    const jumpToSelfUserCenter = ()=>{
        router.push({name:'userCenter'})
    }
    const jumpToLogin = (backAfterSuccess:boolean = true) => {
        router.push({name:'login', params:{backAfterSuccess: backAfterSuccess?'back':undefined}})
    }
    const jumpToRegister = () => {
        router.push({name:'register'})
    }
    const jumpToGlobalAuthGrants = () => {
        router.push({name:'gag'})
    }
    return { jumpToLogin, jumpToUserCenter, jumpToSelfUserCenter, jumpToRegister, jumpToGlobalAuthGrants }
}