export interface IndexQuery{
    Page?:number
    PageSize?:number
    OrderBy?:string
    OrderRev?:boolean
    Search?:string[]
}
export const indexQueryDefault:IndexQuery={
    Page:1,
    PageSize:20
}


export interface IndexResult{
    PageIdx:number,
    PageCount:number,
    TotalCount:number,
    ColumnNames:string[],
    Data:string[][]
}
export const indexResultDefault:IndexResult={
    PageIdx:1,
    PageCount:1,
    TotalCount:1,
    ColumnNames:["加载失败","请检查网络连接","加载失败"],
    Data:[]
}