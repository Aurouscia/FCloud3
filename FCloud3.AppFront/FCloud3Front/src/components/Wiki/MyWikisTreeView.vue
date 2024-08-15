<script setup lang="ts">
import { MyWikisInDir } from '@/models/etc/myWikisOverall';
import { useWikiParsingRoutesJump } from '@/pages/WikiParsing/routes/routesJump'
import foldImg from '@/assets/fold.svg'
import { useFilesRoutesJump } from '@/pages/Files/routes/routesJump';

defineProps<{
    item:MyWikisInDir
}>()
const { jumpToViewWikiRoute } = useWikiParsingRoutesJump()
const { jumpToDirFromIdRoute } = useFilesRoutesJump()
</script>

<template>
<div>
    <div @click="item.unfold = !item.unfold" class="dir" :class="{unfold:item.unfold}">
        <img :src="foldImg"/>
        <span class="n">{{ item.Name }}</span>
        <span class="c">{{ item.Count }}个</span>
        <RouterLink v-show="item.Id>0" :to="jumpToDirFromIdRoute(item.Id)" target="_blank"
            @click="(e:MouseEvent)=>e.stopPropagation()">前往</RouterLink>
    </div>
    <div v-show="item.unfold" class="list">
        <MyWikisTreeView v-for="d in item.Dirs" :item="d"></MyWikisTreeView>
        <div v-for="w in item.Wikis">
            <RouterLink :to="jumpToViewWikiRoute(w[1])" target="_blank">
                {{ w[0] }}
            </RouterLink>
        </div>
    </div>
</div>
</template>

<style scoped lang="scss">
.dir{
    user-select: none;
    font-size: 18px;
    font-weight: bold;
    cursor: pointer;
    img{
        height: 14px;
        transform: rotate(-90deg);
        transition: 0.3s;
    }
    a{
        display: none;
        font-size: 14px;
    }
    .n{
        margin-left: 3px;
    }
    .n:hover{
        text-decoration: underline;
    }
    .c{
        font-size: 14px;
        color: #999;
        margin: 0px 8px 0px 8px;
    }
}
.dir.unfold{
    img{
        transform: rotate(0deg)
    }
    a{
        display: inline;
    }
}
.list{
    margin-left: 6.5px;
    padding-left: 10px;
    border-left: 1px solid #999;
    border-bottom: 1px solid #999;
    padding-bottom: 5px;
}
.list>div{
    min-height: 26px;
    line-height: 26px;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}
</style>