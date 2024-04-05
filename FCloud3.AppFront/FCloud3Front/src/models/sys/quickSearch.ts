export interface QuickSearchResult{
    Items:QuickSearchResultItem[],
    DescIsSrc:boolean
}
export interface QuickSearchResultItem{
    Name:string,
    Desc?:string,
    Id:number
}