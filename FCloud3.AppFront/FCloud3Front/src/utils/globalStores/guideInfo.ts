import { injectApi } from "@/provides";
import { defineStore } from "pinia";
import { ref } from "vue";

export const useGuideInfoStore = defineStore('guideInfo',()=>{
    const api = injectApi()
    const createWikiGuide = ref<string|null|undefined>()
    const introPathName = ref<string|null|undefined>()
    const regulationPathName = ref<string|null|undefined>()
    async function getGuideOf(type:'intro'|'regulation'|'createWiki') {
        if(type=='createWiki'){
            if(createWikiGuide.value === undefined){
                createWikiGuide.value = (await api.etc.guideInfo.createWiki())?.Text
            }
            return createWikiGuide.value
        }
        if(type=='regulation'){
            if(regulationPathName.value === undefined){
                regulationPathName.value = (await api.etc.guideInfo.siteRegulations())?.Text
            }
            return regulationPathName.value
        }
        if(type=='intro'){
            if(introPathName.value === undefined){
                introPathName.value = (await api.etc.guideInfo.siteIntro())?.Text
            }
            return introPathName.value
        }
    }
    return { getGuideOf }
})