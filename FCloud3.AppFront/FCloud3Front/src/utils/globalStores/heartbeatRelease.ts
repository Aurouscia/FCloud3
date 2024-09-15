import { defineStore } from "pinia";
import { ref } from "vue";
import { useRouter } from "vue-router";

export const useHeartbeatReleaseStore = defineStore('heartbeatRelease',()=>{
    const router = useRouter();
    const releaseAction = ref<()=>void>(()=>{})
    const removeGuard = ref<()=>void>(()=>{})
    function registerHeartbeatRelease(){
        removeGuard.value = router.beforeEach(()=>{
            releaseAction.value()
            removeGuard.value()
        })
    }
    return { registerHeartbeatRelease, heartbeatReleaseAction: releaseAction }
})