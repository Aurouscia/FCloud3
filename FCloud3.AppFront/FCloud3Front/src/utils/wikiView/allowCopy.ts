import { WikiParsingResult } from "@/models/wikiParsing/wikiParsingResult"
import { computed, Ref } from "vue"

export const allowCopyParaTrigger = "本段落允许读者复制内容"

export function allowCopy(content:string|undefined){
    return content?.includes(allowCopyParaTrigger) ?? false
}

export const useAllowCopy = (data: Ref<WikiParsingResult|undefined>)=>{

    const allowCopyEachPara = computed<boolean[]>(()=>{
        return data.value?.Paras.map(x => allowCopy(x.Content)) ?? []
    })
    return {
        allowCopyEachPara
    }
}