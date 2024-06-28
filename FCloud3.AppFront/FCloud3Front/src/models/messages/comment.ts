export interface Comment{
    Id:number
    TargetType:CommentTargetType
    TargetObjId:number
    ReplyingTo:number
    Rate:number
    Content:string|null
}

export enum CommentTargetType{
    None = 0,
    Wiki = 1
}


export interface CommentViewResult
{
    Id:number
    Content:string|null
    Hidden:boolean
    Rate:number
    UserId:number
    UserName:string
    UserAvtSrc:string
    Time:string
    Replying:number
    Replies:Array<CommentViewResult>
}

export const cmtTitleId = 666666666;