import { LocalConfigModel } from "../localConfigModel"

export interface WikiContentEditLocalConfig extends LocalConfigModel{
    autoLinkAtSave:boolean
}
export function wikiContentEditConfigDefault(): WikiContentEditLocalConfig {
    return{
        autoLinkAtSave:false,
        key: "wikiContentEdit",
        version: "20241101"
    }
}