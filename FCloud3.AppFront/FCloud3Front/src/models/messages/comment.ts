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
    Rate:number
    UserId:number
    UserName:string
    UserAvtSrc:string
    Time:string
    Replies:Array<CommentViewResult>
}