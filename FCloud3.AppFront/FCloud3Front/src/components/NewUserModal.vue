<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { guideInfo } from '@/utils/guideInfo';
import { useWikiParsingRoutesJump } from '@/pages/WikiParsing/routes/routesJump';

const show = ref<boolean>(false);
const pathName = ref<string|undefined>();
const { jumpToViewWikiRoute } = useWikiParsingRoutesJump()

function setShow(s:boolean){
    show.value = s;
}
defineExpose({setShow})

onMounted(()=>{
    pathName.value = guideInfo.siteRegulations
})
</script>

<template>
    <div v-if="show && pathName" class="fixFillPanelOuter fixFill">
        <div class="fixFillPanelBg fixFill"></div>
        <div class="fixFillPanel">
            <h2>您已成功注册</h2>
            <div class="mainInfo">建议先阅读本站的使用规定<br/>以免被隐藏词条或停用账号</div>
            <div>
                <RouterLink :to="jumpToViewWikiRoute(pathName)" target="_blank">立即查看</RouterLink>
            </div>
        </div>
    </div>
</template>

<style scoped>
</style>