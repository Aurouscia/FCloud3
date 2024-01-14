<script setup lang="ts">
import { inject, onMounted, onUnmounted, ref,Ref} from 'vue';
import { WikiPara, WikiParaRendered} from '../../models/wiki/wikiPara'
import { wikiParaType} from '../../models/wiki/wikiParaTypes'
import { MouseDragListener } from '../../utils/mouseDrag';
import Pop from '../../components/Pop.vue';
import Functions from '../../components/Functions.vue';
import { useRouter } from 'vue-router';
import { Api } from '../../utils/api';
import addIconSrc from '../../assets/add.png';
import dragYIconSrc from '../../assets/dragY.svg';

const paras = ref<Array<WikiParaRendered>>([])
const spaces = ref<Array<number>>([]);
const paraYSpace = 130;
var api:Api;
var pop:Ref<InstanceType<typeof Pop>>;
const router = useRouter();

//临时测试用
const props = {
    wikiId:1
}

function order2PosY(order:number){
    return order*paraYSpace+30;
}
function posY2order(posY:number){
    return Math.round((posY-30)/paraYSpace);
}
function calculatePosY(){
    paras.value.forEach((p) => {
        if(!p.isMoveing){
            p.posY = order2PosY(p.displayOrder||0);
        }
    });
}
function calculateOrderForMoving(){
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
        calculatePosY();
    }
}
var originalOrder:string;
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
            x.Order = x.displayOrder||0;
        })
        calculatePosY();
        moving = false;
        const list = paras.value.map(x=>{return{Id:x.ParaId,Order:x.Order}})
        list.sort((x,y)=>{
            return x.Order-y.Order
        })
        const ids = list.map(x=>x.Id);
        const newOrder = JSON.stringify(ids);
        if(!originalOrder || newOrder!=originalOrder){
            originalOrder = newOrder;
            const resp = await api.wiki.setParaOrders({
                id:props.wikiId,
                orderedParaIds:ids
            })
            setTimeout(()=>{
                refresh(resp);
            },500)
        }
    }
}
async function InsertPara(type:keyof typeof wikiParaType,afterOrder:number){
    const resp = await api.wiki.insertPara({
        id:props.wikiId,
        afterOrder,
        type
    })
    refresh(resp);
}
async function EnterEdit(paraId:number)
{
    const target = paras.value.find(x=>x.ParaId == paraId);
    if(!target){return;}
    if(target.UnderlyingId && target.UnderlyingId>0){
        router.push(`/EditTextSection/${target.UnderlyingId}`);
        return;
    }
    const resp = await api.textSection.createForPara({paraId:paraId});
    if(resp){
        const newlyCreatedId = resp.CreatedId;
        router.push(`/EditTextSection/${newlyCreatedId}`);
        return;
    }
}
async function RemovePara(paraId:number){
    const target = paras.value.find(x=>x.ParaId == paraId);
    if(!target){return;}
    if(window.confirm(`确定要将[${target.Title}]从本词条移除`)){
        const resp = await api.wiki.removePara({
            id:props.wikiId,
            paraId:paraId,
        });
        refresh(resp);
    }
}

async function Load(){
    const resp = await api.wiki.loadSimple(props.wikiId);
    originalOrder = JSON.stringify(resp?.map(x=>x.ParaId))
    refresh(resp);
}
async function refresh(p:WikiPara[]|undefined) {
    if(p){
        paras.value = p;
        paras.value.forEach(x=>x.displayOrder=x.Order);
        spaces.value = new Array<number>(paras.value.length+1)
    }
    calculatePosY();
}

var offsetY = 0;
var moving:boolean = false;
var disposeListeners:()=>void|undefined;
onMounted(async()=>{
    api = inject('api') as Api;
    pop = inject('pop') as Ref<InstanceType<typeof Pop>>;
    await Load();

    const mouse = new MouseDragListener();
    disposeListeners = mouse.startListen(
        (_,y)=>{
            offsetY = y;
            calculateOrderForMoving();
        },
        (_, __)=>{
            endMoving();
        },
        ()=>moving
    );
})
onUnmounted(()=>{
    disposeListeners();
})
</script>

<template>
    <div class="paras" ref="parasDiv">
        <div v-for="p in paras" :key="p.ParaId" class="para" :style="{top:p.posY+'px'}"
        :class="{moving:p.isMoveing}">
            <div class="paraTitle">
                <h2>{{ p.Title }}</h2>
                <img @mousedown="p.isMoveing=true" @touchstart="p.isMoveing=true;moving=true"
                 class="dragY" :src="dragYIconSrc"/>
            </div>
            <div class="paraContent">{{ p.Content }}</div>
            <div class="paraBottom">
                <Functions x-align="right">
                    <button @click="EnterEdit(p.ParaId)">编辑</button>
                    <button>指定已有</button>
                    <button @click="RemovePara(p.ParaId)" class="danger">移除</button>
                </Functions>
            </div>
        </div>
        <div v-for="_,idx in spaces">
            <div class="btnsBetweenPara">
                <Functions :img-src="addIconSrc">
                    <button @click="InsertPara(0,idx - 1)">文本</button>
                    <button @click="InsertPara(1,idx - 1)">文件</button>
                    <button @click="InsertPara(2,idx - 1)">表格</button>
                </Functions>
            </div>
        </div>
    </div>
</template>

<style scoped>
.btnsBetweenPara{
    display: flex;
    height: 60px;
    margin-bottom: 70px;
    justify-content:center;
    align-items: center;
}

.paras {
    min-width: 100%;
    max-width: 600px;
    min-height:1000px;
    padding-bottom: 300px;
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
    text-overflow: ellipsis;
    height: 2em;
}

.paraTitle{
    display: flex;
    justify-content: space-between;
    align-items: center;
}
.paraBottom{
    display: flex;
    justify-content: right;
    align-items: center;
    height: 30px
}
.dragY{
    width: 44px;
    height: 30px;
    object-fit: contain;
    cursor: pointer;
}
</style>../../models/wiki/wikiPara