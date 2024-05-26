import { useRouter } from "vue-router"

export function useWikiRoutesJump(){
    const router = useRouter();
    const jumpToWikiEdit = (urlPathName:string)=>{
        router.push({name:"wikiEdit", params:{urlPathName}})
    }
    return {jumpToWikiEdit}
}