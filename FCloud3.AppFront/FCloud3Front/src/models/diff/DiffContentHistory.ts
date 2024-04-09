export interface DiffContentHistoryResultItem
{
    Id:number
    T:string
    UId:number
    UName:string
    R:number
    A:number
}

export interface DiffContentHistoryResult{
    Items:DiffContentHistoryResultItem[]
}