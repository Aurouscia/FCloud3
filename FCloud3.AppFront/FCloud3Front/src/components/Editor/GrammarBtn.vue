<script setup lang="ts">
import { Grammar, useGrammarBtnStore } from '@/utils/globalStores/grammarBtn';
import { storeToRefs } from 'pinia';

const store = useGrammarBtnStore();
const { clickGrammarBtn } = store;
const { selectedGrammars } = storeToRefs(store)
function glow(type:Grammar){
    return selectedGrammars.value.some(g=>g.type==type)
}
</script>

<template>
<div class="grammarRoot">
    <div class="grammarBtns">
        <div class="i" @click="clickGrammarBtn('italic')" :class="{glow:glow('italic')}">I</div>
        <div class="b" @click="clickGrammarBtn('bold')" :class="{glow:glow('bold')}">B</div>
        <div class="s" @click="clickGrammarBtn('delete')" :class="{glow:glow('delete')}">S</div>
    </div>
    <div style="font-size: 14px;text-align: center;color:#aaa">试验性功能</div>
</div>
</template>

<style scoped lang="scss">
.grammarBtns{
    padding: 5px;
    box-sizing: border-box;
    display: flex;
    justify-content: flex-start;
    align-items: center;
    gap: 3px;
}
.grammarBtns>div{
    flex-shrink: 0;
    width: 25px;
    height: 25px;
    border-radius: 3px;
    line-height: 25px;
    font-size: 20px;
    text-align: center;
    background-color: #eee;
    cursor: pointer;
    user-select: none;
    &:hover{
        background-color: #ddd;
    }
}
.grammarBtns>div.glow{
    background-color: rgb(0, 174, 0);
    color:white;
    &:hover{
        background-color: green;
    }
}
.i,.b,.s{
    font-family: 'Times New Roman', Times, serif;
}
.b{
    font-weight: bold;
}
.i{
    font-style: italic;
}
.s{
    text-decoration: line-through;
}
</style>