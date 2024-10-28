import { defineStore } from "pinia";
import { ref } from "vue";

export const useMainDivDisplayStore = defineStore('mainDivDisplay', ()=>{
    const restrictContentMaxWidth = ref<boolean>(true)
    const displayMarginTop = ref<boolean>(true)
    const enforceScrollY = ref<boolean>(false)
    function resetToDefault(){
        restrictContentMaxWidth.value = true;
        displayMarginTop.value = true;
        enforceScrollY.value = false;
    }
    return { restrictContentMaxWidth, displayMarginTop, enforceScrollY, resetToDefault }
}) 