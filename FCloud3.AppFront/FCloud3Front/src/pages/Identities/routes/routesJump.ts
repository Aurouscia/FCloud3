import { useRouter } from "vue-router"

export function useIdentityRoutesJump(){
    const router = useRouter();
    const jumpToLoginRoute = (backAfterSuccess:boolean | string = true) => {
        let paramValue:string|undefined;
        if(backAfterSuccess === true){
            paramValue = 'back';
        }else if(typeof backAfterSuccess === 'string'){
            paramValue = backAfterSuccess;
        }else{
            paramValue = undefined;
        }
        return {name:'login', params:{backAfterSuccess: paramValue}}
    }
    const jumpToLogin = (backAfterSuccess:boolean | string = true) => {
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
    const jumpToSsoAuthorize = () => {
        router.push({name:'ssoAuthorize'})
    }
    return { jumpToLoginRoute, jumpToLogin, jumpToUserCenter, jumpToUserCenterRoute, jumpToUserCenterFromIdRoute,
        jumpToSelfUserCenter, jumpToRegister, jumpToGlobalAuthGrants, jumpToUserGroup, jumpToSsoAuthorize }
}