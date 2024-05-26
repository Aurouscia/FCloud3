import { useRouter } from "vue-router"
import {  } from "@/models/diff/DiffContentType"

export function useFilesRoutesJump(){
    const router = useRouter();
    const jumpToDirFromId = (id:number)=>{
        router.push({name:'filesFromId', params:{id}})
    }
    return { jumpToDirFromId }
}