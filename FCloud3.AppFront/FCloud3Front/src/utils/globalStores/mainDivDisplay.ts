import { defineStore } from "pinia";
import { ref } from "vue";

export const useMainDivDisplayStore = defineStore('mainDivDisplay', ()=>{
    const restrictContentMaxWidth = ref<boolean>(true)
    const displayMarginTop = ref<boolean>(true)
    function resetToDefault(){
        restrictContentMaxWidth.value = true;
        displayMarginTop.value = true
    }
    return { restrictContentMaxWidth, displayMarginTop, resetToDefault }
}) 