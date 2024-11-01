import { LocalConfigModel } from "../localConfigModel"

export interface AuthLocalConfig extends LocalConfigModel{
    expireHours:number
}
export function authConfigDefault():AuthLocalConfig {
    return{
        expireHours:72,
        key: "auth",
        version: "20240713"
    }
}