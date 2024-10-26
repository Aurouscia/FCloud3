import { defineStore } from "pinia"
import { ref } from "vue"


export type DirInfoType = 'ownerName'|'lastUpdate'|'size'
export const useDirInfoTypeStore = defineStore('dirInfoType', ()=>{
    const infoType = ref<DirInfoType>('ownerName')
    return { infoType }
})