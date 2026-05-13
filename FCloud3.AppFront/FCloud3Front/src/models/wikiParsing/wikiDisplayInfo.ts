export interface WikiDisplayInfo
{
    WikiId: number
    Title: string
    Description: string
    UserName: string
    UserAvtSrc?: string
    Sealed: boolean
    Created: string
    CurrentUserAccess: boolean
    UserGroupLabels: UserGroupLabel[]
    AllowCopy: number
    PolysemyItems: WikiPolysemyItem[]
}
export interface UserGroupLabel{
    Id:number,
    Name:string
}
export interface WikiPolysemyItem { 
    Id: number
    Author: string
    Description: string
    PathName: string
    DirPath: string
}

export const wikiDisplayInfoDefault: WikiDisplayInfo = {
    WikiId:0,
    Title: '',
    Description: '',
    UserName:'',
    UserAvtSrc:undefined,
    Sealed: false,
    Created: 'N/A',
    CurrentUserAccess:false,
    UserGroupLabels:[],
    AllowCopy: 0,
    PolysemyItems: []
}