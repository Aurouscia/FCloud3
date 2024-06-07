import { useRouter } from "vue-router"
import {  } from "@/models/diff/DiffContentType"

export function useFilesRoutesJump(){
    const router = useRouter();
    const jumpToDirFromId = (id:number)=>{
        router.push({name:'filesFromId', params:{id}})
    }
    const jumpToRootDir = (urlPathName?:string)=>{
        router.push({name:'files',params:{path:urlPathName}})
    }
    return { jumpToDirFromId, jumpToRootDir }
}