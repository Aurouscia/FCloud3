export interface QuickSearchResult{
    Items:QuickSearchResultItem[]
}
export interface QuickSearchResultItem{
    Name:string,
    Desc?:string,
    Id:number
}