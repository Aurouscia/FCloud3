import { LocalConfigModel } from "../localConfigModel"

export interface AuthLocalConfig extends LocalConfigModel{
    expireHours:number
}
export const authConfigDefault:AuthLocalConfig = {
    expireHours:72,
    key: "auth",
    version: "20240713"
}