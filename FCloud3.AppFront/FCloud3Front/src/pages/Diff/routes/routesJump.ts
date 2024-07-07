import { useRouter } from "vue-router"
import { DiffContentType, diffContentTypeToStr } from "@/models/diff/diffContentTypes"

export function useDiffRoutesJump(){
    const router = useRouter();
    const jumpToDiffContentHistory = (type:DiffContentType, objId:number)=>{
        router.push({name:'diffContentHistory', params:{type:diffContentTypeToStr(type), objId:objId}})
    }
    return { jumpToDiffContentHistory }
}