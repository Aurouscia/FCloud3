export enum WikiTitleContainType {
    Unknown = 0,
    TextSection = 1,
    FreeTable = 2,
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