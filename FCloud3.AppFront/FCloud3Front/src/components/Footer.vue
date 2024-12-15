<script setup lang="ts">
import { FooterLinks, parseFooterLinks } from '@/models/etc/footerLinks';
import { guideInfo } from '@/utils/guideInfo';
import { onMounted, ref } from 'vue';

const model = ref<FooterLinks>();
const display = ref<boolean>(true);
onMounted(async()=>{
    model.value = parseFooterLinks(guideInfo.footerLinks)
})
defineExpose({display})
</script>

<template>
<div class="footer" v-show="display">
    <div class="vital" v-if="model && model.linksLeft.length>=1">
        <div v-for="item in model?.linksLeft">
            <a :href="item.url||'/'" target="_blank">{{ item.text }}</a>
        </div>
    </div>
    <div class="others">
        <div v-for="item in model?.linksRight">
            <a :href="item.url||'/'" target="_blank">{{ item.text }}</a>
        </div>
    </div>
</div>
</template>

<style scoped lang="scss">
.footer{
    height: 30px;
    font-size: 12px;
    display: flex;
    justify-content: space-between;
    padding: 0px 5px 0px 5px;
}
.footer div{
    line-height: 30px;
}
.vital, .others{
    display: flex;
    justify-content: right;
    flex-wrap: wrap;
    overflow-y: hidden;
    div{
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        margin-left: 5px;
    }
}
.vital{
    flex-shrink: 0;
    flex-wrap: nowrap;
    &>div{
        flex-shrink: 0;
    }
}
a{
    color: #999;
}
</style>