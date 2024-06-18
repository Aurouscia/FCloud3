import { FooterLinks } from "@/models/etc/footerLinks";
import { injectApi } from "@/provides";
import { defineStore } from "pinia";

export function useFooterLinksProvider(){
    const store = useFooterLinksStore();
    const api = injectApi();
    return async()=>{
        if(store.links === undefined){
            store.links = await api.etc.utils.getFooterLinks();
        }
        return store.links;
    }
}

export const useFooterLinksStore = defineStore('footerLinks', {
    state:()=>{
        return {links:undefined as (FooterLinks|undefined)}
    }
})