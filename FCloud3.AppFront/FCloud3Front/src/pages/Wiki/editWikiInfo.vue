<script setup lang="ts">
import { inject, onMounted, ref,Ref} from 'vue';
import { wikiParaSimple} from '../../models/wiki/wikiParaSimple'
import { wikiParaType} from '../../models/wiki/wikiParaTypes'
import { HttpClient,ApiResponse } from '../../utils/httpClient';
import { config } from '../../consts';
import { MouseDragListener } from '../../utils/mouseDrag';
import Pop from '../../components/Pop.vue';

const paras = ref<Array<wikiParaSimple>>([])
const spaces = ref<Array<number>>([]);
const paraYSpace = 130;
var httpClient:HttpClient;
var pop:Ref<InstanceType<typeof Pop>>;

const props = {
    wikiId:4
}

function order2PosY(order:number){
    return order*paraYSpace+30;
}
function posY2order(posY:number){
    return Math.round((posY-30)/paraYSpace);
}
function CalculatePosY(){
    paras.value.forEach((p) => {
        if(!p.isMoveing){
            p.posY = order2PosY(p.displayOrder);
        }
    });
}
function CalculateOrderForMoving(){
    const p = paras.value.find(x=>x.isMoveing);
    const others = paras.value.filter(x=>!x.isMoveing);
    if(p){
        const pposY = order2PosY(p.Order) + offsetY;
        const pOrder = posY2order(pposY);
        others.forEach(x=>{
            if(pOrder>=x.Order && p.Order<x.Order){
                x.displayOrder = x.Order - 1;
            }
            else if(pOrder<=x.Order && p.Order>x.Order){
                x.displayOrder = x.Order + 1;
            }
            else{
                x.displayOrder = x.Order;
            }
        })
        p.posY = pposY;
        CalculatePosY();
    }
}
async function endMoving(){
    const p = paras.value.find(x=>x.isMoveing);
    const others = paras.value.filter(x=>!x.isMoveing);
    if(p){
        const pposY = order2PosY(p.Order) + offsetY;
        const pOrder = posY2order(pposY);
        p.Order = pOrder;
        p.displayOrder = pOrder;
        p.posY = pposY;
        p.isMoveing = false;
        others.forEach(x=>{
            x.Order = x.displayOrder;
        })
        CalculatePosY();
        moving = false;

        const list = paras.value.map(x=>{return{Id:x.Id,Order:x.Order}})
        list.sort((x,y)=>{
            return x.Order-y.Order
        })
        const ids = list.map(x=>x.Id);
        const resp = await httpClient.send(config.api.wikiItem.setParaOrders,{
            Id:props.wikiId,
            OrderedParaIds:ids
        },pop.value.show)
        setTimeout(()=>{
            refresh(resp);
        },500)
    }
}

async function InsertPara(type:keyof typeof wikiParaType,afterOrder:number){
    const resp = await httpClient.send(config.api.wikiItem.insertPara,{
        id:props.wikiId,
        afterOrder,
        type:type
    })
    refresh(resp);
}

async function Load(){
    const resp = await httpClient.send(config.api.wikiItem.loadSimple,{id:props.wikiId},pop.value.show)
    refresh(resp);
}
async function refresh(resp:ApiResponse) {
    if(resp.success){
        paras.value = resp.data as Array<wikiParaSimple>;
        paras.value.forEach(x=>x.displayOrder=x.Order);
        spaces.value = new Array<number>(paras.value.length+1)
    }
    CalculatePosY();
}

var offsetY = 0;
var moving:boolean = false;
onMounted(async()=>{
    httpClient = inject('http') as HttpClient;
    pop = inject('pop') as Ref<InstanceType<typeof Pop>>;
    await Load();

    const mouse = new MouseDragListener();
    mouse.startListen(
        (_,y)=>{
            offsetY = y;
            CalculateOrderForMoving();
        },
        (_, __)=>{
            endMoving();
        },
        ()=>moving
    );
})
</script>

<template>
    <div class="paras" ref="parasDiv">
        <div v-for="p in paras" :key="p.Id" class="para" :style="{top:p.posY+'px'}"
        :class="{moving:p.isMoveing}">
            <div class="paraTitle">
                <h2>{{ p.Title }}</h2>
                <img @mousedown="p.isMoveing=true" @touchstart="p.isMoveing=true;moving=true"
                 class="dragY" src="../../assets/dragY.svg"/>
            </div>
            <div class="paraContent">{{ p.Content }}</div>
            <div>ID:{{ p.Id }}</div>
            <div>Order:{{ p.displayOrder }}</div>
            <div>{{ p.isMoveing }}</div>
        </div>
        <div v-for="_,idx in spaces">
            <div class="btnsBetweenPara">
                <div>
                    <button @click="InsertPara(0,idx - 1)">+文本</button>
                    <button @click="InsertPara(1,idx - 1)">+文件</button>
                    <button @click="InsertPara(2,idx - 1)">+表格</button>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.btnsBetweenPara{
    display: flex;
    height: 50px;
    margin-bottom: 80px;
    justify-content:center;
}
.btnsBetweenPara button{
    z-index: 800;
    position: relative;
    top: 12px;
    opacity: 0;
    transition: 0.2s;
}

.btnsBetweenPara:hover button{
    z-index: 800;
    position: relative;
    top: 12px;
    opacity: 1;
}

.paras {
    min-width: 100%;
    max-width: 600px;
    min-height:1000px;
    position: relative;
    background-color: white;
}

.para {
    position: absolute;
    border-radius: 5px;
    background-color: #eee;
    padding: 10px;
    margin: 10px;
    height: 90px;
    transition: 0.5s;
    width: calc(100% - 40px);
}
.moving{
    background-color: #aaa;
    border: 2px solid #000;
    transition: 0.1s;
    z-index: 100;
}
.paraContent{
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis
}

.paraTitle{
    display: flex;
    justify-content: space-between;
    align-items: center;
}
.dragY{
    width: 30px;
    height: 30px;
    object-fit: contain;
    cursor: pointer;
}
</style>