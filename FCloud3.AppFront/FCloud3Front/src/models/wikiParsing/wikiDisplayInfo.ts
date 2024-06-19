export interface WikiDisplayInfo
{
    WikiId: number
    UserName: string
    UserAvtSrc: string
    Sealed:boolean,
    UserGroupLabels:UserGroupLabel[]
}
export interface UserGroupLabel{
    Id:number,
    Name:string
}

export const wikiDisplayInfoDefault: WikiDisplayInfo = {
    WikiId:0,
    UserName:'',
    UserAvtSrc:'',
    Sealed: false,
    UserGroupLabels:[]
}