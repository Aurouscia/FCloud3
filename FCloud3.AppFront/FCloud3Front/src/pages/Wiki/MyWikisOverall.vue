<script setup lang="ts">
import MyWikisTreeView from '@/components/Wiki/MyWikisTreeView.vue';
import { MyWikisOverallResp } from '@/models/etc/myWikisOverall';
import { injectApi } from '@/provides';
import { onMounted, ref } from 'vue';
import Loading from '@/components/Loading.vue';
import { useWikiParsingRoutesJump } from '../WikiParsing/routes/routesJump';
import { useWikiRoutesJump } from './routes/routesJump';
import { watch } from 'vue';

const api = injectApi()
const props = defineProps<{
    uid?:string
}>()
watch(props, ()=>{
    load()
})

const { jumpToViewWikiRoute } = useWikiParsingRoutesJump()
const { jumpToWikiLocationsRoute } = useWikiRoutesJump()

const data = ref<MyWikisOverallResp>();
async function load(){
    let uid = 0;
    console.log(props.uid)
    if(props.uid){
        uid = parseInt(props.uid)
        if(isNaN(uid))
            uid = 0;
    }
    const resp = await api.etc.myWikis.myWikisOverall(uid);
    if(resp){
        resp.TreeView.unfold = true;
        data.value = resp;
    }
}

onMounted(async()=>{
    await load()
})
</script>

<template>
<h1>我的词条</h1>
<div v-if="data?.TreeView">
    <MyWikisTreeView :item="data?.TreeView"></MyWikisTreeView>
</div>
<Loading v-else></Loading>
<div v-if="data?.HomelessWikis && data.HomelessWikis.length>0">
    <h1>
        无归属词条
        <div class="desc">以下词条没有放入任何目录，请放入目录便于分类整理</div>
    </h1>
    <div class="list">
        <div v-for="w in data.HomelessWikis">
            <RouterLink :to="jumpToWikiLocationsRoute(w[1])" target="_blank" class="put">
                放置
            </RouterLink>
            <RouterLink :to="jumpToViewWikiRoute(w[1])" target="_blank">
                {{ w[0] }}
            </RouterLink>
        </div>
    </div>
</div>
<div v-if="data?.SealedWikis && data.SealedWikis.length>0">
    <h1>
        被隐藏词条
        <div class="desc">以下词条可能违规或不便于展示，请咨询管理员恢复</div>
    </h1>
    <div class="list">
        <div v-for="w in data.SealedWikis">
            <RouterLink :to="jumpToViewWikiRoute(w[1])" target="_blank">
                {{ w[0] }}
            </RouterLink>
        </div>
    </div>
</div>
</template>

<style scoped lang="scss">
.desc{
    font-size: 16px;
    letter-spacing: normal;
    background-color: #999;
    color: white;
    padding: 5px;
    border-radius: 5px;
    width: fit-content;
}
.list>div{
    min-height: 26px;
    line-height: 26px;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}
.put{
    font-size: 14px;
    color: cornflowerblue
}
h1{
    display: flex;
    justify-content: space-between;
    align-items: center;
    flex-wrap: wrap;
    gap: 5px;
}
</style>