import { DiffContentType, diffContentTypeToStr } from "@/models/diff/diffContentTypes"

export function useDiffRoutesJump(){
    const jumpToDiffContentHistoryRoute = (type:DiffContentType, objId:number)=>{
        return {name:'diffContentHistory', params:{type:diffContentTypeToStr(type), objId:objId}}
    }
    const jumpToDiffContentHistoryForWikiRoute = (wikiPathName:string)=>{
        return {name:'diffContentHistoryForWiki', params:{wikiPathName}}
    }
    return { jumpToDiffContentHistoryRoute, jumpToDiffContentHistoryForWikiRoute }
}