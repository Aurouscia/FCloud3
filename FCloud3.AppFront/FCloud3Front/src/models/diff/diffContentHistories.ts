export interface DiffContentHistoryResultItem
{
    Id:number
    T:string
    UId:number
    UName:string
    R:number
    A:number
    H:boolean
}

export interface DiffContentHistoryResult{
    Items:DiffContentHistoryResultItem[]
}