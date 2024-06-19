<script setup lang="ts">
import { FooterLinks } from '@/models/etc/footerLinks';
import { useFooterLinksProvider } from '@/utils/globalStores/footerLinks';
import { onMounted, ref } from 'vue';

const model = ref<FooterLinks>();
const display = ref<boolean>(true);
const getFooterLinks = useFooterLinksProvider();
onMounted(async()=>{
    model.value = await getFooterLinks();
})
defineExpose({display})
</script>

<template>
<div class="footer" v-show="display">
    <div class="vital" v-if="model && model.Links.length>=1">
        <a :href="model.Links[0].Url||'/'" target="_blank">{{ model.Links[0].Text }}</a>
    </div>
    <div class="others">
        <div v-for="item,idx in model?.Links">
            <a v-if="idx>0" :href="item.Url||'/'" target="_blank">{{ item.Text }}</a>
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
.vital{
    flex-shrink: 0;
}
.others{
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
a{
    color: #999;
}
</style>