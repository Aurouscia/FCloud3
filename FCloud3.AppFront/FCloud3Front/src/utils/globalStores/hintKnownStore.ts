import { defineStore } from "pinia";
import { ref } from "vue";

export const useHintKnownStore = defineStore('hintKnown', ()=>{
    const wikiCanBeExported = ref(false)
    return { wikiCanBeExported }
}, {
    persist: {
        key: 'fcloud3-hintKnown'
    }
})
