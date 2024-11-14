import axios, { Axios, AxiosResponse } from 'axios'
import _ from 'lodash'
import {AxiosError} from 'axios'

export type ApiResponse = {
    success: boolean
    data: any
    errmsg: string
    code: number
}
export type RequestType = "get"|"postForm"|"postRaw"|"getStream";

export type HttpCallBack = (result:"ok"|"warn"|"err",msg:string)=>void
export interface ApiRequestHeader{
    Authorization:string|undefined
}

export const ApiResponseCodes = 
{
    Normal: 0,
    NoTourist: 70827
}

const storageKey = "fcloudAuthToken"
const defaultFailResp:ApiResponse = {data:undefined,success:false,errmsg:"失败",code:0}

export class HttpClient{
    jwtToken:string|null=null
    httpCallBack:HttpCallBack
    unauthorizeCallBack:()=>void
    needMemberCallBack:()=>void
    showWaitCallBack:(s:boolean)=>void
    ax:Axios
    constructor(httpCallBack:HttpCallBack, unauthorizeCallBack:()=>void, needMemberCallBack:()=>void, showWait:(s:boolean)=>void){
        this.jwtToken = localStorage.getItem(storageKey);
        this.httpCallBack = httpCallBack;
        this.unauthorizeCallBack = unauthorizeCallBack;
        this.needMemberCallBack = needMemberCallBack;
        this.showWaitCallBack = showWait;
        this.ax = axios.create({
            baseURL: import.meta.env.VITE_ApiUrlBase,
            validateStatus: (n)=>n < 500
          });
    }
    setToken(token:string){
        this.jwtToken = token;
        localStorage.setItem(storageKey,token);
    }
    clearToken(){
        this.jwtToken = null;
        localStorage.removeItem(storageKey);
    }
    private headers(){
        return {
            Authorization: `Bearer ${this.jwtToken}`
        }
    }
    private showErrToUser(err:AxiosError){
        console.log(err);
        if(err.status){
            const codeText = this.statusCodeText(err.status);
            if(codeText){
                this.httpCallBack("err", codeText);
                return;
            }
        }
        this.httpCallBack("err","请检查网络连接");
    }
    async request(resource:string, type:RequestType, data?:any, successMsg?:string, showWait?:boolean): Promise<ApiResponse>{
        console.log(`开始发送[${type}]=>[${resource}]`,data)
        let res:AxiosResponse|undefined = undefined;
        if(showWait)
            this.showWaitCallBack(true);
        try{
            if(type=='get' || type=='getStream'){
                res = await this.ax.get(
                    resource,
                    {
                        params:data,
                        headers:this.headers(),
                    }
                );
            }else if(type=='postRaw'){
                res = await this.ax.post(
                    resource,
                    data,
                    {
                        headers:this.headers()
                    }
                );
            }else if(type=='postForm'){
                res = await this.ax.postForm(resource,
                    data,
                    {
                        headers:this.headers()
                    }
                );
            }
            if(showWait)
                this.showWaitCallBack(false);
        }
        catch(ex){
            if(showWait)
                this.showWaitCallBack(false);
            const err = ex as AxiosError;
            console.log(`[${type}]${resource}失败`,err)
            this.showErrToUser(err);
        }
        if (res) {
            if (res.status == 401) {
                this.httpCallBack("err","请登录")
                this.unauthorizeCallBack()
                return defaultFailResp;
            }

            let resp: ApiResponse|undefined = undefined;
            if (this.isApiResponseObj(res.data)) {
                resp = res.data as ApiResponse;
            } else if (type=='getStream') {
                resp = {
                    success: true,
                    errmsg: '',
                    code: 0,
                    data: res.data,
                }
            }
            else {
                resp = defaultFailResp;
            }
            if (resp.success) {
                console.log(`[${type}]${resource}成功`, resp.data)
                if (successMsg) {
                    this.httpCallBack('ok', successMsg)
                }
            }
            if (!resp.success) {
                console.log(`[${type}]${resource}失败`, resp.errmsg || res.statusText);
                if (resp.errmsg) {
                    this.httpCallBack('err', resp.errmsg);
                } else {
                    const codeText = this.statusCodeText(res.status);
                    this.httpCallBack('err', codeText || "未知错误")
                }
            }
            if (resp.code == ApiResponseCodes.NoTourist) {
                this.needMemberCallBack()
            }
            return resp;
        }
        return defaultFailResp;
    }
    statusCodeText(code:number|undefined|null){
        if(code == 401){
            return "请登录";
        }
        if(code == 403){
            return "无权限";
        }
        if(code||0 >= 500){
            return "服务器未知错误";
        }
        return undefined;
    }
    private isApiResponseObj(obj:any){
        const c = (propName:keyof ApiResponse)=>{
            return _.has(obj, propName)
        }
        if(typeof obj == 'object'){
            return c('code') && c('success') && c('errmsg')
        }
    }
}