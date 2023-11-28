import { Http,ApiResponse,FileType } from "./http";
import { popDelegate} from '../components/Pop.vue'

export type RequestType = "get"|"postForm"|"postRaw";
export interface Api{
    type:RequestType,
    reletiveUrl:string
}

const storageKey = "fcloudAuthToken"

export type {ApiResponse,FileType}
export class HttpClient{
    jwtToken:string|null=null
    constructor(){
        this.jwtToken = localStorage.getItem(storageKey)
    }
    setToken(token:string){
        this.jwtToken = token;
        localStorage.setItem(storageKey,token);
    }
    clearToken(){
        this.jwtToken = null;
        localStorage.removeItem(storageKey);
    }
    private getHeaders():Array<[string,string]>{
        const headers:Array<[string,string]> = []
        if(this.jwtToken){
            headers.push(["Authorization",`Bearer ${this.jwtToken}`])
        }
        return headers;
    }
    get(resource:string,data?:any): Promise<ApiResponse>{
        if(!data)
            return Http.get(resource,this.getHeaders());
        else{
            var safeCounter = 0;
            var q = "?"
            for(const key in data){
                q = q+key;
                q = q+"=";
                q = q+String(data[key]);
                q = q+"&";
                safeCounter++;
                if(safeCounter>6){
                    break;
                }
            }
            return Http.get(resource+q,this.getHeaders());
        }
    }
    postAsJson(resource:string,data:object): Promise<ApiResponse>{
        return Http.postAsJson(resource,data,this.getHeaders())
    }
    postAsForm(resource:string,data:object): Promise<ApiResponse>{
        return Http.postAsForm(resource,data,this.getHeaders())
    }
    async send(apiInfo:Api,data?:object,
        pop?:popDelegate,
        successMsg?:string
        ): Promise<ApiResponse>
        {
        var resp:ApiResponse;
        if(apiInfo.type=="get"){
            resp = await this.get(apiInfo.reletiveUrl,data)
        }else if(apiInfo.type=="postForm"){
            resp = await this.postAsForm(apiInfo.reletiveUrl,data||{});
        }else if(apiInfo.type=="postRaw"){
            resp = await this.postAsJson(apiInfo.reletiveUrl,data||{});
        }else{
            throw new Error("未实现的请求类型");
        }
        if(pop){
            if(!resp.success){
                pop(resp.errmsg,"failed")
            }else if(successMsg){
                pop(successMsg,"success")
            }
        }
        return resp;
    }
}