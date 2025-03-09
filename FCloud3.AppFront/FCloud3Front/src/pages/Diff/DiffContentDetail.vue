<script setup lang="ts">
import { nextTick, onMounted, onUnmounted, ref, watch } from 'vue';
import { DiffContentStepDisplay } from '@/models/diff/diffContentDetails';
import { TimedLock } from '@/utils/timeStamp';
import { isFireFox } from '@/utils/browserInfo';

const props = defineProps<{
    display:DiffContentStepDisplay|undefined
}>();

const from = ref<HTMLDivElement>();
const to = ref<HTMLDivElement>();
let leftDoms:HTMLElement[] = [];
let rightDoms:HTMLElement[] = [];

watch(props, async()=>{
    await refreshDisplay();
})
onMounted(async()=>{
    await refreshDisplay();
})

async function refreshDisplay(){
    await nextTick();
    let counter = 0;
    const addedRanges:Range[] = []
    leftDoms = [];
    props.display?.To.forEach(f=>{
        const dom = document.getElementById(domId('to', counter++))
        if(!dom || !dom.childNodes[0]){
            return []
        }
        leftDoms.push(dom)
        const ranges:Range[] = f.High.map(([start,end])=>{
            const range = document.createRange();
            range.setStart(dom.childNodes[0], start);
            range.setEnd(dom.childNodes[0], end);   
            return range;
        })
        addedRanges.push(...ranges);
    })
    counter = 0;
    const removedRanges:Range[] = []
    rightDoms = []
    props.display?.From.forEach(f=>{
        const dom = document.getElementById(domId('from', counter++))
        if(!dom || !dom.childNodes[0]){
            return []
        }
        rightDoms.push(dom)
        const ranges:Range[] = f.High.map(([start,end])=>{
            const range = document.createRange();
            range.setStart(dom.childNodes[0], start);
            range.setEnd(dom.childNodes[0], end);
            return range;
        })
        removedRanges.push(...ranges);
    })
    const addedHighlight = new Highlight(...addedRanges);
    const removedHighlight = new Highlight(...removedRanges);
    if(!isFireFox){
        CSS.highlights.clear()
        CSS.highlights.set('added', addedHighlight);
        CSS.highlights.set('removed', removedHighlight);
    }
    ensureSameHeight();
}
function ensureSameHeight(){
    if(leftDoms.length == rightDoms.length){
        for(let i=0;i<leftDoms.length;i++){
            const from = leftDoms[i];
            const to = rightDoms[i];
            from.style.height = "";
            to.style.height = "";
            const fromHeight = from.offsetHeight;
            const toHeight = to.offsetHeight;
            const targetHeight = Math.max(fromHeight, toHeight)
            from.style.height = targetHeight+"px";
            to.style.height = targetHeight+"px"
        }
    }
}
function domId(side:"from"|"to", idx:number){
    return `df_${side}_${idx}`;
}

let fromFollowing = false;
let toFollowing = false;
let fromTimer = 0;
let toTimer = 0;
const scrollLock = new TimedLock(10);
async function scrollHandler(role:"from"|"to"){
    if(!scrollLock.isOk()){return;}
    if(role=='from'){
        if(fromFollowing){return}
        toFollowing = true;
        to.value!.scrollTop = from.value!.scrollTop
        clearTimeout(toTimer);
        toTimer = window.setTimeout(()=>{toFollowing=false},100)
    }else{
        if(toFollowing){return}
        fromFollowing = true;
        from.value!.scrollTop = to.value!.scrollTop
        clearTimeout(fromTimer);
        fromTimer = window.setTimeout(()=>{fromFollowing=false},100)
    }
}

const resizeLock = new TimedLock(50);
function windowResizeHandler(){
    if(resizeLock.isOk()){
        ensureSameHeight();
    }
}
onMounted(()=>{
    window.addEventListener("resize",windowResizeHandler)
})
onUnmounted(()=>{
    window.removeEventListener("resize",windowResizeHandler)
})
</script>

<template>
<div class="diffContentDetail">
    <div v-show="!display?.Hidden" class="from" ref="from" @scroll="scrollHandler('from')">
        <div v-for="t,idx in display?.From" :id="domId('from', idx)">
        {{ t.Text }}
        </div>
    </div>
    <div v-show="!display?.Hidden" class="to" ref="to" @scroll="scrollHandler('to')">
        <div v-for="t,idx in display?.To" :id="domId('to', idx)">
        {{ t.Text }}
        </div>
    </div>
    <div v-show="display?.Hidden" class="hiddenNotice">
        该编辑记录已被隐藏，可能违法或包含隐私信息<br/>仅管理员可见，如有查看需要请咨询管理员
    </div>
</div>
</template>

<style scoped lang="scss">
.diffContentDetail{
    display: flex;
    gap:10px;
    flex-direction: row;
    justify-content: center;
    align-items: center;
    flex-grow: 1;
}
.from,.to{
    height: 100%;
    width: 50%;
    flex-grow: 1;
    overflow-x: hidden;
    overflow-y: scroll;
    white-space: pre-wrap;
    box-sizing: border-box;
    padding: 10px;
    background-color: #eee;
    word-break: break-all;
    div{
        margin-bottom: 20px;
        background-color: #fefefe;
        padding: 5px;
    }
}
@media screen and ( max-width: 1100px ){
    .diffContentDetail{
        flex-direction: column;
    }
    .from,.to{
        width: 100%;
        height: 50%;
    }
}
.hiddenNotice{
    width: 100%;
    height: 100%;
    background-color: #eee;
    text-align: center;
    display: flex;
    justify-content: center;
    align-items: center;
}

::highlight(added) {
  background-color: green;
  color: white;
}
::highlight(removed) {
  background-color: red;
  color: white;
}
</style>