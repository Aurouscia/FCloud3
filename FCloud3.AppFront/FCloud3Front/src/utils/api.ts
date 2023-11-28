import { popDelegate } from "../components/Pop.vue";
import { User } from "../models/identities/user";
import { TextSection } from "../models/textSection/textSection";
import { WikiPara } from "../models/wiki/wikiPara";
import { wikiParaType } from "../models/wiki/wikiParaTypes";
import { HttpClient } from "./httpClient";
import { IdentityInfo } from "./userInfo";

export class Api{
    private httpClient: HttpClient;
    constructor(httpClient:HttpClient){
        this.httpClient = httpClient;
    }
    identites = {
        login: async(reqObj:{userName:string,password:string},pop?:popDelegate)=>{
            var res = await this.httpClient.send({
                reletiveUrl:"/api/Auth/Login",
                type:"postForm"
            },reqObj,pop,"已成功登录");
            if(res.success){
                return res.data["token"] as string;
            }
        },
        identityTest: async(pop?:popDelegate)=>{
            var res = await this.httpClient.send({
                reletiveUrl:"/api/Auth/IdentityTest",
                type:"get"
            },undefined,pop)
            if(res.success){
                return res.data as IdentityInfo
            }
        },
        edit: async(pop?:popDelegate)=>{
            var res = await this.httpClient.send({
                reletiveUrl:"/api/User/Edit",
                type:"get"
            },undefined,pop)
            if(res.success){
                return res.data as User
            }
        },
        editExe: async(user:User, pop?:popDelegate)=>{
            var res = await this.httpClient.send({
                reletiveUrl:"/api/User/EditExe",
                type:"postRaw"
            },user,pop,"修改成功")
            if(res.success){
                return true
            }
        },
    }
    wiki = {
        create: undefined,
        edit:undefined,
        editExe:undefined,
        loadSimple: async(id:number,pop?:popDelegate)=>{
            const res = await this.httpClient.send({
                reletiveUrl:"/api/WikiItem/LoadSimple",
                type:"get"
            }
            ,{id:id},pop);
            if(res.success){
                return res.data as Array<WikiPara>
            }
        },
        insertPara:async(req:{id:number,afterOrder:number,type:keyof typeof wikiParaType},pop?:popDelegate) => {
            const res = await this.httpClient.send({
                reletiveUrl:"/api/WikiItem/InsertPara",
                type:"postForm"
            },req,pop,"成功插入新段落")
            if(res.success){
                return res.data as Array<WikiPara>
            }
        },
        setParaOrders:async(req:{id:number,orderedParaIds:number[]},pop:popDelegate)=>{
            const res = await this.httpClient.send({
                reletiveUrl:"/api/WikiItem/SetParaOrders",
                type:"postRaw"
            },req,pop,"成功修改顺序")
            if(res.success){
                return res.data as Array<WikiPara>
            }
        }
    }
    textSection = {
        createForCorr:async (req:{corrId:number},pop:popDelegate) => {
            const res = await this.httpClient.send({
                reletiveUrl:"/api/TextSection/CreateForCorr",
                type:"postForm"
            },req,pop)
            if(res.success){
                return res.data as {CreatedId:number}
            }
        },
        edit:async(id:number,pop:popDelegate)=>{
            const res = await this.httpClient.send({
                reletiveUrl:"/api/TextSection/Edit",
                type:"get"
            },{id:id},pop)
            if(res.success){
                return res.data as TextSection
            }
        },
        editExe:async(textSection:TextSection,pop:popDelegate)=>{
            const res = await this.httpClient.send({
                reletiveUrl:"/api/TextSection/EditExe",
                type:"postRaw"
            },textSection,pop,"编辑成功")
            if(res.success){
                return res.data as boolean;
            }
        },
    }
}