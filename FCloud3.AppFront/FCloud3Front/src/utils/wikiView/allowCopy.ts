import { WikiParsingResult } from "@/models/wikiParsing/wikiParsingResult"
import { computed, Ref } from "vue"

export const useAllowCopy = (data: Ref<WikiParsingResult|undefined>)=>{
    const allowCopyParaTrigger = "本段落允许读者复制内容"

    const allowCopyEachPara = computed<boolean[]>(()=>{
        return data.value?.Paras.map(x => x.Content?.includes(allowCopyParaTrigger) ?? false) ?? []
    })
    return {
        allowCopyEachPara
    }
}