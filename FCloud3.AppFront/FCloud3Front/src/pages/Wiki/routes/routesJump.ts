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
    const jumpToWikiLocationsRoute = (urlPathName?:string)=>{
        return {name:"wikiLocations", params:{urlPathName}}
    }
    const jumpToWikiContentSearch = ()=>{
        router.push({name:'wikiContentSearch'})
    }
    const jumpToMyWikisOverall = (uid?:number)=>{
        router.push({name:'myWikisOverall', params:{uid}})
    }
    const jumpToViewParaRawContentRoute = (paraId:number)=>{
        return {name:"viewParaRawContent", params:{paraId}}
    }
    const jumpToWikiOpRecordRoute = (wikiId:number)=>{
        return {name:'wikiOpRecord', params:{wikiId}}
    }
    return {jumpToWikiEdit, jumpToWikiContentEdit, jumpToWikiLocations,
        jumpToWikiLocationsRoute, jumpToWikiContentSearch, jumpToMyWikisOverall,
        jumpToViewParaRawContentRoute, jumpToWikiOpRecordRoute
    }
}