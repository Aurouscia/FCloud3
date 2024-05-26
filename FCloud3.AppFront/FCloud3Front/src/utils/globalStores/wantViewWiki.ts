import { defineStore } from "pinia";

export const useWantViewWikiStore = defineStore('wantViewWiki', {
    state:()=>{
        return {
            wantViewWiki:undefined as string|undefined
        }
    },
    actions:{
        set(urlPathName:string){
            this.wantViewWiki = urlPathName
        },
        readAndReset(){
            const w = this.wantViewWiki;
            this.wantViewWiki = undefined;
            return w;
        }
    }
})