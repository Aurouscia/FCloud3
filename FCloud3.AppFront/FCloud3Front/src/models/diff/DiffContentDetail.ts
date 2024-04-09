export interface DiffContentDetailResult{
    Items: DiffContentDetailResultItem[]
}

export interface DiffContentDetailResultItem
{
    Id:number
    Content:string
    Added:[number,number][]
    Removed:[number,number][]
}