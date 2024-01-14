import axios, { Axios } from 'axios'
import {AxiosError} from 'axios'

export type ApiResponse = {
    success: boolean
    data: any
    errmsg: string
}
export type RequestType = "get"|"postForm"|"postRaw";
export interface Api{
    type:RequestType,
    reletiveUrl:string
}
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
        this.httpCallBack("err",err.message);
    }
    async request(resource:string,type:RequestType,data?:any,successMsg?:string): Promise<ApiResponse>{
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
            this.showErrToUser(err);
        }
        if(res){
            const resp = res.data as ApiResponse;
            if(resp.success && successMsg){
                this.httpCallBack('ok',successMsg)
            }
            if(!resp.success){
                this.httpCallBack('err',resp.errmsg)
            }
            return resp;
        }else{
            return defaultFailResp;
        }
    }
}