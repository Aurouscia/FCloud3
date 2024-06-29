import { useRouter } from "vue-router"

export function useWikiRoutesJump(){
    const router = useRouter();
    const jumpToWikiEdit = (urlPathName:string)=>{
        router.push({name:"wikiEdit", params:{urlPathName}})
    }
    const jumpToWikiContentEdit = (urlPathName?:string)=>{
        if(!urlPathName){
            return
        }
        router.push({name:'wikiContentEdit', params:{urlPathName}})
    }
    const jumpToWikiLocations = (urlPathName?:string)=>{
        if(!urlPathName){
            return;
        }
        router.push({name:"wikiLocations", params:{urlPathName}})
    }
    return {jumpToWikiEdit, jumpToWikiContentEdit, jumpToWikiLocations}
}