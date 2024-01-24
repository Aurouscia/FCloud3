import axios, { Axios } from 'axios'
import {AxiosError} from 'axios'

export type ApiResponse = {
    success: boolean
    data: any
    errmsg: string
}
export type RequestType = "get"|"postForm"|"postRaw";

export type HttpCallBack = (result:"ok"|"warn"|"err",msg:string)=>void
export interface ApiRequestHeader{
    Authorization:string|undefined
}

const storageKey = "fcloudAuthToken"
const defaultFailResp:ApiResponse = {data:undefined,success:false,errmsg:"失败"}

export class HttpClient{
    jwtToken:string|null=null
    httpCallBack:HttpCallBack
    ax:Axios
    constructor(httpCallBack:HttpCallBack){
        this.jwtToken = localStorage.getItem(storageKey);
        this.httpCallBack = httpCallBack;
        this.ax = axios.create({
            baseURL: import.meta.env.VITE_BASEURL,
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
        if(err.response?.status==401){
            this.httpCallBack("err","请登录");
            return;
        }
        this.httpCallBack("err","请检查网络连接");
    }
    async request(resource:string,type:RequestType,data?:any,successMsg?:string): Promise<ApiResponse>{
        console.log(`开始发送[${type}]=>[${resource}]`,data)
        var res;
        try{
            if(type=='get'){
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
        }
        catch(ex){
            const err = ex as AxiosError;
            console.log(`[${type}]${resource}失败`,err)
            this.showErrToUser(err);
        }
        if(res){
            const resp = res.data as ApiResponse;
            if(resp.success){
                console.log(`[${type}]${resource}成功`,resp.data)
                if(successMsg){
                    this.httpCallBack('ok',successMsg)
                }
            }
            if(!resp.success){
                console.log(`[${type}]${resource}失败`,resp.errmsg||res.statusText);
                if(res.status==401){
                    this.httpCallBack('err','请登录');
                }else{
                    this.httpCallBack('err',resp.errmsg)
                }
            }
            return resp;
        }else{
            return defaultFailResp;
        }
    }
}