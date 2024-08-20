import { WikiParaType } from "./wikiParaType"

export enum WikiTitleContainType {
    Unknown = 0,
    TextSection = 1,
    FreeTable = 2,
}
export function paraType2ContainType(type:WikiParaType){
    if(type==WikiParaType.Text)
        return WikiTitleContainType.TextSection
    else if(type==WikiParaType.Table)
        return WikiTitleContainType.FreeTable
    return WikiTitleContainType.Unknown
}
export interface WikiTitleContainListModel {
    Items: WikiTitleContainListModelItem[]
}
export interface WikiTitleContainListModelItem {
    Id: number
    WikiTitle: string
    WikiId: number
} 


export interface WikiTitleContainSetAllRequest
{
    Type: WikiTitleContainType
    ObjectId: number
    WikiIds: number[]
}
export interface WikiTitleContainGetAllRequest{
    Type: WikiTitleContainType
    ObjectId: number
}

export interface WikiTitleContainAutoFillResult{
    Items:WikiTitleContainAutoFillResultItem[]
}
export interface WikiTitleContainAutoFillResultItem{
    Id:number,
    Title:string
}