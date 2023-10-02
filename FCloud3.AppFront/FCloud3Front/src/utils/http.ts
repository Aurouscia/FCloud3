export type ApiResponse = {
    success: boolean
    data: any
    errmsg: string
}
export type FileType = "any"

function fullUrl(resource: string) {
    const base = import.meta.env.VITE_BASEURL
    if (!resource.startsWith("/")) {
        resource = "/" + resource
    }
    return base + resource;
}
async function postFormData(url: string, send: FormData, headers: Array<[string, string]>):Promise<ApiResponse>{
    try {
        const resp = await fetch(url, {
            method: "post",
            body: send,
            headers: new Headers(headers)
        });
        const text = await resp.text();
        if(text){
            const res:ApiResponse = JSON.parse(text);
            console.log(`向${url} post数据，返回:`, res)
            return res;
        }
        var errmsg = "与服务器通讯出错";
        if(resp.status>=401 && resp.status<500 && resp.status!=404){
            errmsg = "没有权限";
        }
        return{
            success:false,
            data:undefined,
            errmsg
        }
    }
    catch (e) {
        console.log(`向${url} post出现问题:${e}`);
        return {
            success: false,
            data: {},
            errmsg: "与服务器通讯出错"
        };
    }
}

async function postRawJson(url: string, send: object, headers: Array<[string, string]>):Promise<ApiResponse>{
    try {
        headers.push(["Content-Type","application/json"]);
        const resp = await fetch(url, {
            method: "post",
            body: JSON.stringify(send),
            headers: new Headers(headers)
        });
        const text = await resp.text();
        if(text){
            const res:ApiResponse = JSON.parse(text);
            console.log(`向${url} post数据，返回:`, res)
            return res;
        }
        var errmsg = "与服务器通讯出错";
        if(resp.status==401){
            errmsg = "请先登录"
        }
        else if(resp.status>=401 && resp.status<500 && resp.status!=404){
            errmsg = "没有权限";
        }
        return{
            success:false,
            data:undefined,
            errmsg
        }
    }
    catch (e) {
        console.log(`向${url} post出现问题:${e}`);
        return {
            success: false,
            data: {},
            errmsg: "与服务器通讯出错"
        };
    }
}

export const Http = {
    get: async function get(resource: string, headers: Array<[string, string]>): Promise<ApiResponse> {
        const url = fullUrl(resource)
        try {
            const resp = await fetch(url, {
                headers: new Headers(headers)
            });
            const text = await resp.text();
            if(text){
                const res:ApiResponse = JSON.parse(text);
                console.log(`向${url} get数据，返回:`, res)
                return res;
            }
            var errmsg = "与服务器通讯出错";
            if(resp.status>=401 && resp.status<500){
                errmsg = "没有权限";
            }
            return{
                success:false,
                data:undefined,
                errmsg
            }
        }
        catch (e) {
            console.log(`向${url}资源请求出现问题:${e}`);
            return {
                success: false,
                data: {},
                errmsg: "与服务器通讯出错"
            };
        }
    },
    postAsJson: async function postAsJson(resource: string, data: object, headers: Array<[string, string]>): Promise<ApiResponse> {
        const url = fullUrl(resource);
        return postRawJson(url, data, headers)
    },
    postAsForm: async function postAsForm(resource: string, data: object, headers: Array<[string, string]>): Promise<ApiResponse> {
        const url = fullUrl(resource);
        const send = new FormData();
        const keys = Object.keys(data);
        const values = Object.values(data);
        for (var i = 0; i < keys.length; i++) {
            send.append(keys[i], values[i])
        }
        return postFormData(url,send,headers);
    },
    postFile:async function postFile(resource:string="/api/files/upload", file:File, fileType:FileType, headers: Array<[string, string]>):Promise<ApiResponse> {
        const url = fullUrl(resource);
        const send = new FormData();
        send.append("file",file);
        send.append("fileType",fileType);
        return postFormData(url,send,headers);
    }
}