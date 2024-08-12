type MyWikisWikiNamePath = [string?, string?]
export interface MyWikisOverallResp
{
    TreeView:MyWikisInDir
    HomelessWikis: MyWikisWikiNamePath[]
}
export interface MyWikisInDir
{
    Id:number
    Name?:string
    Count:number
    Wikis:MyWikisWikiNamePath[]
    Dirs?:MyWikisInDir[]

    unfold?:boolean
}