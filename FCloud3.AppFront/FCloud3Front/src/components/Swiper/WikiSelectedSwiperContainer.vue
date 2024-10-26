<script setup lang="ts">
import Swiper from '@/components/Swiper/Swiper.vue';
import { SwiperData } from '@/components/Swiper/swiperData';
import { useWikiParsingRoutesJump } from '@/pages/WikiParsing/routes/routesJump';
import { injectApi } from '@/provides';
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

const props = defineProps<{
    width?:number
    height?:number
}>()
const data = ref<SwiperData>({
    items:[
    ],
    width: props.width,
    height: props.height
})

const api = injectApi()
const router = useRouter()
const { jumpToViewWikiFromIdRoute } = useWikiParsingRoutesJump()
async function load(){
    const res = await api.wiki.wikiSelected.getList()
    if(res){
        data.value.items = res.map(ws=>{
            const linkRoute = jumpToViewWikiFromIdRoute(ws.WikiItemId)
            const link = router.resolve(linkRoute).href
            return{
                title:ws.WikiTitle||' ',
                link:link,
                imgUrl:ws.CoverUrl||'/fcloud.svg',
                desc:ws.Intro||' '
            }
        })
    }
}
onMounted(async()=>{
    await load()
})
</script>

<template>
<Swiper v-if="data && data.items.length>0" :data="data" class="swiperOuter"></Swiper>
</template>

<style scoped lang="scss">
.swiperOuter{
    margin: auto;
}
</style>