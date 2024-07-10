export interface LocalConfigModel{
    key:LocalConfigType
    version:string
}
export type LocalConfigType = "textSection"

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

const defaultVals = [textSectionConfigDefault];

export function readLocalConfig(type:LocalConfigType){
    const val = defaultVals.find(x=>x.key == type);
    if(val){
        return readLocalConfigByModel(val);
    }
}
const key = (k:string) => `localConfig_${k}`
export function readLocalConfigByModel(model:LocalConfigModel){
    const dataStr = localStorage.getItem(key(model.key))
    if(!dataStr)
        return model;
    const data = JSON.parse(dataStr);
    if(data.version !== model.version){
        return model;
    }
    return data as LocalConfigModel;
}
export function saveLocalConfig(model:LocalConfigModel){
    localStorage.setItem(key(model.key), JSON.stringify(model));
}