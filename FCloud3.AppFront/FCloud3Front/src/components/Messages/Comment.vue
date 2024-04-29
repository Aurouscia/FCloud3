<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue';
import { Comment, CommentTargetType, CommentViewResult } from '../../models/messages/comment';
import { injectApi, injectPop } from '../../provides';
import CommentItem from './CommentItem.vue';

const props = defineProps<{
    type:CommentTargetType
    objId:number
}>()
const comments = ref<Array<CommentViewResult>>([]);
const writing = ref<string>();
const rate = ref<number>(0);

async function load(){
    const resp = await api.messages.comment.view(props.type, props.objId)
    if(resp){
        comments.value = resp;
    }
}
async function send() {
    if(rate.value == 0 && !writing.value){
        pop.value.show("不能提交空评论", "failed");
        return;
    }
    const cmt:Comment = {
        Id: 0,
        TargetType: props.type,
        TargetObjId: props.objId,
        Rate:rate.value,
        Content:writing.value || null,
        ReplyingTo:0
    }
    const resp = await api.messages.comment.create(cmt)
    if(resp){
        rate.value = 0;
        writing.value = "";
    }
    await load();
}
function rateColor(pos: number) {
    if (pos==-1 && rate.value==0){
        return "#666"
    }
    if (pos >= rate.value) {
        return "#ccc"
    }
    if (rate.value <= 2){
        return "red"
    }
    if (rate.value <= 5){
        return "orange"
    }
    if (rate.value <= 8){
        return "cornflowerblue"
    }
    return "green"
}
const rateText = computed(()=>{
    if (rate.value == 0){
        return "评分"
    }
    if (rate.value <= 2){
        return "太差"
    }
    if (rate.value <= 5){
        return "一般"
    }
    if (rate.value <= 8){
        return "不错"
    }
    return "超棒"
})
async function inputKeyDown(e:KeyboardEvent){
    console.log(e.key)
    if(e.key=="Enter"){
        await send();
    }
}

let api = injectApi()
let pop = injectPop()
onMounted(async ()=>{
    await load();
})
</script>

<template>
    <div class="give">
        <div class="rate">
            <div class="rates">
                <div @mouseenter="rate=0" style="width:38px;" :style="{backgroundColor: rateColor(-1)}">{{ rateText }}</div>
                <div v-for="_,idx in Array(10)" @mouseenter="rate = idx+1" :style="{backgroundColor: rateColor(idx)}">
                    {{ idx + 1 }}
                </div>
            </div>
        </div>
        <div class="write">
            <input v-model="writing" placeholder="写评论(最多200字)" @keydown="inputKeyDown">
            <button class="ok" @click="send">发送</button>
        </div>
    </div>
    <div class="comments">
        <CommentItem v-for="c in comments" :c="c" :objType="type" :objId="objId" @needLoad="load"></CommentItem>
    </div>
</template>

<style scoped lang="scss">
.give{
    background-color: #eee;
    padding: 10px;
}
.write{
    margin: 10px 0px 0px 0px;
    display: flex;
    gap: 5px;
    align-items: center;
    input{
        padding: 4px;
        box-sizing: border-box;
        flex-grow: 1;
    }
}
.rates{
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 2px;
    div{
        width: 28px;
        height: 24px;
        line-height: 24px;
        text-align: center;
        background-color: #aaa;
        color: white;
        border-radius: 3px;
        cursor: pointer;
        transition: 0.5s;
    }
}
</style>