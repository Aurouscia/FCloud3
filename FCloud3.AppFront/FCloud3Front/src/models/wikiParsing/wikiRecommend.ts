export interface WikiRecommendModel{
    Dirs: Dir[]
    Wikis: Wiki[]
}

export interface Dir{
    Id: number
    Name: string
}
export interface Wiki
{
    Title: string
    UrlPathName: string
}