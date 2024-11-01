import { LocalConfigModel } from "../localConfigModel"

export interface TextSectionLocalConfig extends LocalConfigModel{
    blackBg:boolean,
    fontSize:number,
    autoLinkAtSave:boolean
}
export const textSectionConfigDefault:TextSectionLocalConfig = {
    blackBg: true,
    fontSize: 16,
    autoLinkAtSave:false,
    key: "textSection",
    version: "20240622"
}