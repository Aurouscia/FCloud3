export interface WikiRecommendModel{
    Dirs: Dir[]
}

export interface Dir{
    Id: number
    Name: string
    TotalWikiCount: number
    Wikis: Wiki[]
}
export interface Wiki
{
    Title: string
    UrlPathName: string
}