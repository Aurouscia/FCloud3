import { LocalConfigModel } from "../localConfigModel"

export interface TextSectionLocalConfig extends LocalConfigModel{
    blackBg:boolean,
    fontSize:number,
}
export function textSectionConfigDefault():TextSectionLocalConfig {
    return{
        blackBg: true,
        fontSize: 16,
        key: "textSection",
        version: "20240622"
    }
}