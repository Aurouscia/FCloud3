<script setup lang="ts">
import { ref, computed } from 'vue';
import { WikiPolysemyItem } from '@/models/wikiParsing/wikiDisplayInfo';
import { useWikiParsingRoutesJump } from '@/pages/WikiParsing/routes/routesJump';

const props = defineProps<{
    description?: string|null;
    items: WikiPolysemyItem[];
}>()

const expanded = ref(false)
const count = computed(() => props.items.length)
const { jumpToViewWikiRoute } = useWikiParsingRoutesJump()

function toggleExpand() {
    expanded.value = !expanded.value
}
</script>

<template>
<div class="polysemySelector" v-if="props.description || count > 0">
    <div class="topRow">
        <div class="currentInfo">
            <span class="currentDesc">{{ props.description || '请联系作者补充描述' }}</span>
            <span v-if="count > 0" class="divider"></span>
            <button v-if="count > 0" class="expandBtn" @click="toggleExpand">
                {{ expanded ? '收起' : `展开` }}同名({{ count }})
            </button>
        </div>
    </div>
    <div v-if="expanded" class="itemList">
        <div class="hint">本词条是一个多义词，请在下列义项中选择</div>
        <div v-for="item in props.items" :key="item.Id" class="item">
            <RouterLink :to="jumpToViewWikiRoute(item.PathName)">
                <span class="itemTitle">{{ item.Description || item.Author+'的作品' }}</span>
            </RouterLink>
        </div>
    </div>
</div>
</template>

<style lang="scss" scoped>
.polysemySelector{
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;
    border-radius: 4px;
}
.topRow{
    display: flex;
    flex-direction: row;
    align-items: center;
    justify-content: center;
    gap: 10px;
}
.currentInfo{
    display: flex;
    flex-direction: row;
    align-items: center;
    gap: 8px;
    flex-wrap: nowrap;
    padding: 0px 10px;
    .currentDesc{
        font-size: 14px;
        flex-shrink: 1;
        color: #999;
        word-break: break-all;
    }
    .divider{
        width: 0px;
        height: 16px;
        border-left: 1px solid #ccc;
    }
    .expandBtn{
        font-size: 13px;
        flex-shrink: 0;
        padding: 0;
        background: none;
        border: none;
        color: cornflowerblue;
        cursor: pointer;
        &:hover{
            text-decoration: underline;
        }
    }
}
.itemList{
    display: flex;
    flex-direction: column;
    gap: 3px;
    padding: 8px;
    border-radius: 10px;
    box-shadow: 0px 0px 3px 0px #ccc;
    .hint{
        font-size: 14px;
        color: #888;
        margin-bottom: 6px;
    }
}
.item{
    gap: 10px;
    padding: 4px 8px;
    border-radius: 4px;
    background-color: #f5f5f5;
    font-size: 14px;
    a{
        display: block;
    }
}
</style>
