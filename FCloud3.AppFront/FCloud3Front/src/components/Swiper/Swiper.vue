<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, useTemplateRef } from 'vue';
import { defaultHeight, defaultWidth, SwiperData } from './swiperData';
import { watchWindowWidth } from '@/utils/eventListeners/windowSizeWatcher';

const props = defineProps<{
    data:SwiperData
}>()
const outerDiv = useTemplateRef('outerDiv')
const unitWidth = ref<number>(defaultWidth)
const unitHeight = ref<number>(defaultHeight)
const widthStyle = computed<string>(() => unitWidth.value+'px')
const widthTotalStyle = computed<string>(() => unitWidth.value*props.data.items.length+'px')
const heightStyle = computed<string>(() => unitHeight.value+'px')

const focusUnitIdx = ref<number>(0)
const movedUnits = ref<number>(0)
const maxMovedUnit = computed<number>(()=>props.data.items.length-1)
const movedPx = computed<number>(() => movedUnits.value*unitWidth.value)
const innerLeftStyle = computed<string>(() => -movedPx.value+'px')

const moveHandleMinMs = 20
const cycleMs = 10

const dragging = ref<boolean>(false)
let inputStartAtX: number
let lastMoveHandle: number = 0
let lastHandleDraggedUnits: number = 0;
let speed: number = 0;
function userInputHandler(type:'start'|'move'|'end', e:MouseEvent|TouchEvent){
    let intv = 0;
    if(type=='move'){
        if(!dragging.value){
            return
        }
        //e.preventDefault() 会影响页面浏览
        let time = new Date().getTime()
        intv = time - lastMoveHandle
        if(intv < moveHandleMinMs)
            return
        designated = false
        lastMoveHandle = time
    }
    if(type=='end'){
        dragging.value = false;
        lastHandleDraggedUnits = 0;
        return;
    }
    let x:number;
    if('pageX' in e){
        x = e.pageX
    }else{
        x = e.touches[0].pageX
    }
    if(type=='start'){
        inputStartAtX = x
        dragging.value = true
        return;
    }
    const draggedPx = x - inputStartAtX
    const draggedUnits = draggedPx/unitWidth.value
    movedUnits.value = focusUnitIdx.value - draggedUnits
    movedUnitsRestrict()
    if(Math.abs(draggedUnits)>0.1){
        speed = (lastHandleDraggedUnits-draggedUnits)/intv
        lastHandleDraggedUnits = draggedUnits;
    }
}

let designated = false;
function cycleAction(){
    autoMove()
    if(dragging.value)
        return;
    let moveBy = 0;
    if(Math.abs(speed)>0.001){
        moveBy = speed*cycleMs
        if(speed>0.005){
            speed = 0.005;
        }else if(speed<-0.005){
            speed = -0.005
        }
        //穷人固定伤害，富人百分比伤害
        if(Math.abs(speed)<0.002)
            speed -= Math.sign(speed)*0.0001
        else
            speed *= 0.93
    }
    else{
        const diff = movedUnits.value - focusUnitIdx.value
        moveBy = -diff/10;
        if(Math.abs(moveBy)<0.0001){
            moveBy = -diff
        }
    }
    movedUnits.value += moveBy
    movedUnitsRestrict()
    if(!designated)
        focusUnitIdx.value = Math.round(movedUnits.value)
    else{
        if(focusUnitIdx.value == Math.round(movedUnits.value))
            designated = false;
    }
}
function movedUnitsRestrict(){
    if(movedUnits.value < -0.1){
        movedUnits.value = -0.1
        speed = 0
    }else if(movedUnits.value > maxMovedUnit.value+0.1){
        movedUnits.value = maxMovedUnit.value+0.1
        speed = 0;
    }
}
function moveTo(idx:number, postponedAutoMove?:boolean){
    designated = true
    if(idx<0){
        idx=0
    }else if(idx>=props.data.items.length){
        idx=0
    }
    focusUnitIdx.value = idx
    if(postponedAutoMove){
        const now = (new Date()).getTime()
        noAutoMoveBefore = now + autoMoveRecover
    }
}

let noAutoMoveBefore = 0
let firstAutoMoved = false;
const autoMoveIntv = 2000
const autoMoveRecover = 10000
function autoMove(){
    const now = (new Date()).getTime()
    if(dragging.value){
        noAutoMoveBefore = now + autoMoveRecover
        return
    }
    if(now<noAutoMoveBefore){
        return;
    }
    if(firstAutoMoved){
        moveTo(focusUnitIdx.value+1)
    }else{
        firstAutoMoved = true
    }
    noAutoMoveBefore = now+autoMoveIntv
}
function calWidth(){
    const parent = outerDiv.value?.parentElement
    if(parent){
        unitWidth.value = parent.clientWidth
    }
    else
        unitWidth.value = defaultWidth
    if(props.data.width && unitWidth.value>props.data.width){
        unitWidth.value = props.data.width
    }
}

let mouseMoveReg:(e:MouseEvent)=>void
let touchMoveReg:(e:TouchEvent)=>void
let mouseUpReg:(e:MouseEvent)=>void
let touchEndReg:(e:TouchEvent)=>void
let cycleTimer:number|undefined
let removeWindowSizeWatcher:(()=>void)|undefined
onMounted(()=>{
    calWidth()
    removeWindowSizeWatcher = watchWindowWidth(calWidth, 500)
    unitHeight.value = props.data.height || defaultHeight
    
    mouseMoveReg = e=>userInputHandler('move',e)
    mouseUpReg = e=>userInputHandler('end', e)
    touchMoveReg = e=>userInputHandler('move',e)
    touchEndReg = e=>userInputHandler('end', e)
    window.addEventListener('mousemove', mouseMoveReg)
    window.addEventListener('mouseup', mouseUpReg)
    window.addEventListener('touchmove', touchMoveReg, {passive:false})
    window.addEventListener('touchend', touchEndReg)
    cycleTimer = window.setInterval(cycleAction, cycleMs)
})
onUnmounted(()=>{
    window.removeEventListener('mousemove', mouseMoveReg)
    window.removeEventListener('mouseup', mouseUpReg)
    window.removeEventListener('touchmove', touchMoveReg)
    window.removeEventListener('touchend', touchEndReg)
    window.clearInterval(cycleTimer)
    if(removeWindowSizeWatcher)
        removeWindowSizeWatcher();
})
</script>

<template>
<div class="swiper" ref="outerDiv" :style="{width:widthStyle, height:heightStyle}"
    @mousedown="e=>userInputHandler('start', e)" @touchstart="e=>userInputHandler('start', e)">
    <div class="swiperInner" :style="{width:widthTotalStyle, left:innerLeftStyle}">
        <div v-for="i in data.items" class="swiperItem" 
            :style="{width:widthStyle, height:heightStyle}">
            <img :src="i.imgUrl"/>
            <div class="swiperText">
                <div class="swiperTextTitle"><a :href="i.link" target="_blank">{{ i.title }}</a></div>
                <div>{{ i.desc }}</div>
            </div>
        </div>
    </div>
    <div class="swiperDots">
        <div v-for="_,idx in data.items" :class="{activeDot:idx===focusUnitIdx}" @click="moveTo(idx, true)"></div>
    </div>
</div>
</template>

<style scoped lang="scss">
*{
    user-select: none;
}
.swiper{
    box-sizing: border-box;
    overflow: hidden;
    border-radius: 10px;
    position: relative;
}
.swiperInner{
    display: flex;
    flex-direction: row;
    position: absolute;
}
.swiperItem{
    position: relative;
    img{
        position: absolute;
        width: 100%;
        height: 100%;
        inset: 0px;
        z-index: -1;
        object-fit: cover;
    }
}
.swiperText{
    position: absolute;
    width: 100%;
    box-sizing: border-box;
    text-align: left;
    height: 130px;
    bottom: 0px;
    background: linear-gradient(to bottom, rgba(0, 0, 0, 0), rgba(0,0,0,0.5) 50%, rgba(0, 0, 0, 0.7) 100%);
    padding: 50px 10px 20px 10px;
    color:white;
    display: flex;
    flex-direction: column;
    justify-content: flex-end;
    font-size: 14px;
    gap: 5px;
    div{
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }
    .swiperTextTitle{
        font-size: 20px;
    }
    a{
        color: white;
    }
}
.swiperDots{
    position: absolute;
    bottom: 0px;
    left: 0px;
    right: 0px;
    height: 20px;
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 5px;
    div{
        height: 8px;
        width: 8px;
        border-radius: 1000px;
        border: 1px solid white;
        cursor: pointer;
    }
    div.activeDot{
        background-color: white;
    }
}
</style>