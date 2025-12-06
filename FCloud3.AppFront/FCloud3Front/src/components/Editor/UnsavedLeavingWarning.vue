<script setup lang="ts">
import { ref } from 'vue';

const props = defineProps<{
    release: ()=>void
}>();
const noSave = ref<boolean>(false);
function ok(){
    if(noSave.value){
        props.release();
    }
    emits('ok')
}
const emits = defineEmits<{
    (e:'ok'):void
}>()
</script>

<template>
    <div class="fixFillPanelOuter fixFill">
        <div class="fixFillPanelBg fixFill"></div>
        <div class="fixFillPanel">
            <h2>警告</h2>
            <div>
                有未保存的更改，离开前应先保存
            </div>
            <div class="noSave">
                <input v-model="noSave" type="checkbox"> 我要不保存直接离开
            </div>
            <div>
                <button :class="noSave?'danger':'ok'" @click="ok">OK</button>
            </div>
        </div>
    </div>
</template>

<style scoped>
input{
    width: 25px;
    height: 25px;
}
.noSave{
    display: flex;
    align-items: center;
}
</style>