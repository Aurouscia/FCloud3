import { IndexQuery, IndexResult } from "../models";
import { User } from "../models/identities/user";
import { TextSection, TextSectionPreviewResponse } from "../models/textSection/textSection";
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
        login: async(reqObj:{userName:string,password:string})=>{
            var res = await this.httpClient.request(
                "/api/Auth/Login",
                "postForm",
                reqObj,
                "已成功登录");
            if(res.success){
                return res.data["token"] as string;
            }
        },
        identityTest: async()=>{
            var res = await this.httpClient.request(
                "/api/Auth/IdentityTest",
                "get")
            if(res.success){
                return res.data as IdentityInfo
            }
        },
        edit: async()=>{
            var res = await this.httpClient.request(
                "/api/User/Edit",
                "get")
            if(res.success){
                return res.data as User
            }
        },
        editExe: async(user:User)=>{
            var res = await this.httpClient.request(
                "/api/User/EditExe",
                "postRaw",
                user,
                "修改成功")
            if(res.success){
                return true
            }
        },
        getInfoByName: async(name:string)=>{
            var res = await this.httpClient.request(
                "/api/User/GetInfoByName",
                "get",
                {name:name}
            )
            if(res.success){
                return res.data as User
            }
        }
    }
    wiki = {
        create: undefined,
        edit:undefined,
        editExe:undefined,
        loadSimple: async(id:number)=>{
            const res = await this.httpClient.request(
                "/api/WikiItem/LoadSimple",
                "get",
                {id:id});
            if(res.success){
                return res.data as Array<WikiPara>
            }
        },
        insertPara:async(req:{id:number,afterOrder:number,type:keyof typeof wikiParaType}) => {
            const res = await this.httpClient.request(
                "/api/WikiItem/InsertPara",
                "postForm",
                req,
                "成功插入新段落")
            if(res.success){
                return res.data as Array<WikiPara>
            }
        },
        setParaOrders:async(req:{id:number,orderedParaIds:number[]})=>{
            const res = await this.httpClient.request(
                "/api/WikiItem/SetParaOrders",
                "postRaw",
                req,
                "成功修改顺序")
            if(res.success){
                return res.data as Array<WikiPara>
            }
        },
        removePara:async(req:{id:number,paraId:number})=>{
            const res = await this.httpClient.request(
                "/api/WikiItem/RemovePara",
                "postForm",
                req,
                "成功删除")
            if(res.success){
                return res.data as Array<WikiPara>
            }
        },
        index:async(req:IndexQuery)=>{
            const res = await this.httpClient.request(
                "/api/WikiItem/Index",
                "postRaw",
                req,
            )
            if(res.success){
                return res.data as IndexResult;
            }
        }
    }
    textSection = {
        createForPara:async (req:{paraId:number}) => {
            const res = await this.httpClient.request(
                "/api/TextSection/CreateForPara",
                "postForm",
                req)
            if(res.success){
                return res.data as {CreatedId:number}
            }
        },
        edit:async(id:number)=>{
            const res = await this.httpClient.request(
                "/api/TextSection/Edit",
                "get"
                ,{id:id})
            if(res.success){
                return res.data as TextSection
            }
        },
        editExe:async(textSection:TextSection)=>{
            const res = await this.httpClient.request(
                "/api/TextSection/EditExe",
                "postRaw",
                textSection,
                "编辑成功")
            if(res.success){
                return res.data as boolean;
            }
        },
        preview:async(textSecId:number,content:string)=>{
            const res = await this.httpClient.request(
                "/api/TextSection/Preview",
                "postForm",
                {id:textSecId,content:content});
            if(res.success){
                return res.data as TextSectionPreviewResponse;
            }
        }
    }
}