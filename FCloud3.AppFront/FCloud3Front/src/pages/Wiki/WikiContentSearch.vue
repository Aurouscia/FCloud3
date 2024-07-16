<script setup lang="ts">
import { WikiContentSearchResult } from '@/models/etc/wikiContentSearchResult';
import { injectApi } from '@/provides';
import { nextTick } from 'vue';
import { ref } from 'vue';

const data = ref<WikiContentSearchResult>();
async function search(){
    const res = await api.etc.searchWikiContent.search(str.value);
    if(res){
        data.value = res;
        await nextTick();
        const ranges:Array<Range> = []
        let wCounter = 0;
        let pCounter = 0;
        let tCounter = 0;
        data.value.WikiItems.forEach(w=>{
            pCounter = 0;
            w.Paras.forEach(p=>{
                tCounter = 0;
                p.Took.forEach(t=>{
                    const dom = document.getElementById(domId(wCounter, pCounter, tCounter))
                    if(!dom || !dom.childNodes[0]){
                        return []
                    }
                    t.High.forEach(([start,end])=>{
                        const range = document.createRange();
                        range.setStart(dom.childNodes[0], start);
                        range.setEnd(dom.childNodes[0], end);
                        ranges.push(range);
                    })
                    tCounter++;
                })
                pCounter++;
            })
            wCounter++;
        })
        const addedHighlight = new Highlight(...ranges);
        console.log(ranges.length)
        CSS.highlights.set('target', addedHighlight);
    }
}
function domId(widx:number, pIdx:number, tIdx:number){
    return `${widx}_${pIdx}_${tIdx}`;
}
function inputKeydownHandler(e:KeyboardEvent){
    if(e.key == "Enter"){
        search();
    }
}

const str = ref("");
const api = injectApi();

</script>

<template>
<h1>词条正文搜索</h1>
<div class="contentSearch">
    <div class="write">
        <input v-model="str" maxlength="32" placeholder="搜索正文内容" @keydown="inputKeydownHandler"/>
        <button @click="search">搜索</button>    
    </div>
    <span v-if="!data" class="hint">暂不支持直接跳转到位置，请在词条页面使用浏览器的页面内搜素功能</span>
    <div class="result" v-if="data">
        <div v-for="i,wIdx in data.WikiItems" class="w">
            <a class="wTitle" :href="`/#/w/${i.WikiUrlPathName}`" target="_blank">{{ i.WikiTitle }}</a>
            <div v-for="p,pIdx in i.Paras" class="p">
                <div class="pTitle">{{ p.ParaTitle }}</div>
                <div v-for="t,tIdx in p.Took" :id="domId(wIdx, pIdx, tIdx)">
                    {{ t.Text }}
                </div>
            </div>
        </div>
    </div>
</div>
</template>

<style scoped lang="scss">
::highlight(target) {
  background-color: green;
  color: white;
}

.hint{
    color: gray;
    font-size: 14px;
}
.pTitle{
    font-size: 16px;
    font-weight: bold;
    margin-bottom: 5px;
}
.p{
    margin-top: 4px;
    padding: 4px;
    background-color: #f8f8f8;
}
.wTitle{
    font-size: 20px;
    font-weight: bold;
    margin-bottom: 5px;
    cursor: pointer;
    &:hover{
        text-decoration: underline;
    }
}
.w{
    padding: 8px;
    background-color: #e8e8e8;
}
.result{
    width: 100%;
    display: flex;
    flex-direction: column;
    gap: 10px;
    margin-bottom: 30px;
}
.write{
    display: flex;
    justify-content: center;
    align-items: center;
    input{
        font-size: 18px;
    }
}
.contentSearch{
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: start;
}
</style>