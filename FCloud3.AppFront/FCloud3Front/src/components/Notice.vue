<script setup lang="ts">
import { onMounted, ref } from 'vue';

const props = defineProps<{
    type:"info"|"warn"|"danger",
    title?:string
}>();
const title = ref<string>();
onMounted(()=>{
    if(!props.title){
        if(props.type=='info'){
            title.value = "⚠ 注意"
        }else if(props.type=='warn'){
            title.value = "⚠ 注意"
        }else if(props.type=='danger'){
            title.value = "⚠ 警告"
        }
    }else{
        title.value = props.title;
    }
})
</script>

<template>
    <div class="notice" :class="props.type">
        <div class="title">{{ title }}</div>
        <slot></slot>
    </div>
</template>

<style scoped>
.title{
    font-size: 20px;
    padding-bottom: 3px;
    margin-bottom: 3px;
}
.notice.danger{
    background-color: plum;
    border: 2px solid red;
    color:white
}
.notice.warn{
    background-color: rgb(248, 226, 186);
    border: 2px solid orange;
    color:rgb(174, 113, 0)
}
.notice.info{
    background-color: #bbb;
    border:2px solid #666;
    color:white
}
.notice{
    border-radius: 5px;
    padding: 10px;
    font-size: 14px;
    margin: 5px;
    white-space: wrap;
    text-align: left;
}
</style>