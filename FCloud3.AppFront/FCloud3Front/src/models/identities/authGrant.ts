export interface AuthGrant{
    Id:number
    ToId:number
    To:AuthGrantTo
    OnId:number
    On:AuthGrantOn
    IsReject:boolean
}

export interface AuthGrantViewModelItem extends AuthGrant{
    ToName:string
    CreatorName:string
}

export interface AuthGrantViewModel{
    BuiltIn: AuthGrantViewModelItem[]
    Global: AuthGrantViewModelItem[]
    Local: AuthGrantViewModelItem[]
}

export enum AuthGrantOn{
    None = 0,

    WikiItem = 10,
    WikiPara = 20,
    TextSection = 21,
    FreeTable = 22,

    Dir = 30,
    FileItem = 31,
    Material = 32,

    User = 40,
    UserGroup = 41
}
export enum AuthGrantTo{
    None = 0,
    User = 1,
    UserGroup = 2,
    EveryOne = 3,
    SameGroup = 4
}