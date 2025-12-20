import { useRouter } from "vue-router"

export function useIdentityRoutesJump(){
    const router = useRouter();
    const jumpToLoginRoute = (backAfterSuccess:boolean = true) => {
        return {name:'login', params:{backAfterSuccess: backAfterSuccess?'back':undefined}}
    }
    const jumpToLogin = (backAfterSuccess:boolean = true) => {
        router.push(jumpToLoginRoute(backAfterSuccess))
    }
    const jumpToUserCenter = (username:string) => {
        router.push({name:'userCenter', params:{username}})
    }
    const jumpToUserCenterRoute = (username:string) => {
        return {name:'userCenter', params:{username}}
    }
    const jumpToUserCenterFromIdRoute = (uid:number) => {
        return {name:'userCenterFromId', params:{uid}}
    }
    const jumpToSelfUserCenter = ()=>{
        router.push({name:'userCenter'})
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
    return { jumpToLoginRoute, jumpToLogin, jumpToUserCenter, jumpToUserCenterRoute, jumpToUserCenterFromIdRoute,
        jumpToSelfUserCenter, jumpToRegister, jumpToGlobalAuthGrants, jumpToUserGroup }
}