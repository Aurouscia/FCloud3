<script setup lang="ts">
import { inject, onBeforeMount, ref } from 'vue';
import FileUpload from '../../components/FileUpload.vue';
import Search from '../../components/Search.vue';
import SwitchingTabs from '../../components/SwitchingTabs.vue';
import { Api } from '../../utils/api';

const props = defineProps<{
    paraId?:number
}>();

const search = ref<InstanceType<typeof Search>>();
async function setFileId(id:number){
    if(!props.paraId){return;}
    const res = await api.wiki.para.setFileParaFileId(props.paraId, id);
    if(res){
        emit('fileIdSet');
        search.value?.clear();
    }
}

var api:Api;
onBeforeMount(()=>{
    //如果mount之后再inject会造成Search组件认为api是undefined
    api = inject('api') as Api;
})
const emit = defineEmits<{
    (e:'fileIdSet'):void
}>()
</script>

<template>
    <div class="wikiFileParaEdit">
        <SwitchingTabs :texts="['选择已有','新上传']">
            <div>
                <Search ref="search" :source="api.utils.quickSearch.fileItem" @done="(_val,id)=>setFileId(id)"></Search>
            </div>
            <FileUpload :dist="'wikiFile'" :single="true" @uploaded="setFileId">
            </FileUpload>
        </SwitchingTabs>
    </div>
</template>

<style scoped>

</style>