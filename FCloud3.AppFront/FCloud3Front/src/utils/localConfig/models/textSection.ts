import { LocalConfigModel } from "../localConfigModel"

export interface TextSectionLocalConfig extends LocalConfigModel{
    blackBg:boolean,
    fontSize:number,
    autoLinkAtSave:boolean
}
export function textSectionConfigDefault():TextSectionLocalConfig {
    return{
        blackBg: true,
        fontSize: 16,
        autoLinkAtSave:false,
        key: "textSection",
        version: "20240622"
    }
}