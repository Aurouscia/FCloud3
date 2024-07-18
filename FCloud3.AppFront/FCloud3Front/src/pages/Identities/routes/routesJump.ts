import { useRouter } from "vue-router"

export function useIdentityRoutesJump(){
    const router = useRouter();
    const jumpToUserCenter = (username:string) => {
        router.push({name:'userCenter', params:{username}})
    }
    const jumpToUserCenterFromIdRoute = (uid:number) => {
        return {name:'userCenterFromId', params:{uid}}
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
    const jumpToUserGroup = (id:number)=>{
        router.push({name:'userGroup',params:{id}})
    }
    return { jumpToLogin, jumpToUserCenter, jumpToUserCenterFromIdRoute,
        jumpToSelfUserCenter, jumpToRegister, jumpToGlobalAuthGrants, jumpToUserGroup }
}