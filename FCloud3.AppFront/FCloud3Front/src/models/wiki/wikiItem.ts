export interface WikiItem{
    Id:number,
    Title:string,
    UrlPathName:string
}

export interface WikiInDirLocationView{
    WikiId:number
    Title:string
    Locations:WikiInDirLocationItem[]
}
export interface WikiInDirLocationItem{
    Id:number,
    NameChain:string
}