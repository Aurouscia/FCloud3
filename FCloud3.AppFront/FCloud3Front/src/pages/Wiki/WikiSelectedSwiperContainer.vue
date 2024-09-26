<script setup lang="ts">
import Swiper from '@/components/Swiper/Swiper.vue';
import { SwiperData } from '@/components/Swiper/swiperData';

const data:SwiperData = {
    items:[
        {
            imgUrl: 'http://static.wiki.jowei19.com/wikiFile/404252700.PNG',
            title: '笃初人民共和国',
            desc: '架空国家设定典范佳作',
            link: 'http://wiki.jowei19.com/#/w/xin-zhong-hai-jie-dao'
        },
        {
            imgUrl: 'http://static.wiki.jowei19.com/wikiFile/lex25n0e.jpg',
            title: '新中海街道',
            desc: '架空街道区划设定典范佳作',
            link: 'http://wiki.jowei19.com/#/w/xin-zhong-hai-jie-dao'
        },
        {
            imgUrl: 'http://static.wiki.jowei19.com/wikiFile/615844825.png',
            title: '十里国际机场',
            desc: '架空机场设定典范佳作',
            link: 'http://wiki.jowei19.com/#/w/xin-zhong-hai-jie-dao'
        }
    ],
    width: 500,
    height: 200
}
</script>

<template>
<Swiper :data="data" class="swiperOuter"></Swiper>
</template>

<style scoped lang="scss">
.swiperOuter{
    margin: auto;
}
</style>