<script setup lang="ts">
import { useWikiParsingRoutesJump } from '@/pages/WikiParsing/routes/routesJump';
import { injectApi } from '@/provides';
import Notice from '../Notice.vue';
import { useUrlPathNameConverter } from '@/utils/urlPathName';

const props = defineProps<{
    inDirId?:number,
    noH1?:boolean
}>();
const {jumpToViewWiki} = useWikiParsingRoutesJump();
const {name:creatingWikiTitle, converted:creatingWikiUrlPathName, run:runForWiki} = useUrlPathNameConverter();

const api = injectApi();
async function createWiki() {
    if(props.inDirId === undefined){
        const resp = await api.wiki.wikiItem.create(creatingWikiTitle.value||"", creatingWikiUrlPathName.value||"")
        if(resp){
            jumpToViewWiki(creatingWikiUrlPathName.value);
        }
    }
    else{
        const resp = await api.wiki.wikiItem.createInDir(creatingWikiTitle.value||"", creatingWikiUrlPathName.value||"", props.inDirId)
        if(resp){
            jumpToViewWiki(creatingWikiUrlPathName.value);
        }
    }
}
</script>

<template>
    <h1 v-if="!noH1">新建词条</h1>
    <table>
        <tr>
            <td>词条<br />标题</td>
            <td><input v-model="creatingWikiTitle" placeholder="必填" /></td>
        </tr>
        <tr>
            <td>链接<br />名称</td>
            <td>
                <div>
                    <button class="minor" @click="runForWiki">由标题自动生成</button>
                </div>
                <input v-model="creatingWikiUrlPathName" placeholder="必填" spellcheck="false" />
            </td>
        </tr>
        <tr class="noneBackground">
            <td colspan="2">
                <button class="confirm" @click="createWiki">确认</button>
            </td>
        </tr>
    </table>
    <Notice type="warn">
        请谨慎设置链接名称，每次修改将导致旧链接失效
    </Notice>
</template>