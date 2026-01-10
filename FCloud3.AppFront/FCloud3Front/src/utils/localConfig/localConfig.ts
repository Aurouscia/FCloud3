import { LocalConfigModel, LocalConfigType } from "./localConfigModel";
import { authConfigDefault } from "./models/auth";
import { textSectionConfigDefault } from "./models/textSection";
import { cloneDeep } from 'lodash'
import { wikiContentEditConfigDefault } from "./models/wikiContentEdit";
import { wikiCenteredHomePageLocalConfigDefault } from "./models/wikiCenteredHomePage";

const defaultVals = [
    textSectionConfigDefault(),
    authConfigDefault(),
    wikiContentEditConfigDefault(),
    wikiCenteredHomePageLocalConfigDefault()
];

const key = (k:string) => `localConfig_${k}`
export function readLocalConfig(type:LocalConfigType){
    const defaultVal = getLocalConfigDefaultVal(type)
    if(!defaultVal)
        return
    const dataStr = localStorage.getItem(key(type))
    if(!dataStr)
        return defaultVal;
    let data:any;
    try{
        data = JSON.parse(dataStr);
    }
    catch{}
    if(data && data.version && data.version === defaultVal.version){
        return data as LocalConfigModel;
    }
    return defaultVal;
}

export function getLocalConfigDefaultVal(type:LocalConfigType){
    const val = defaultVals.find(x=>x.key == type);
    if(val){
        return cloneDeep(val)
    }
}

export function saveLocalConfig(model:LocalConfigModel){
    localStorage.setItem(key(model.key), JSON.stringify(model));
}