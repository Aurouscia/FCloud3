export interface UserGroup{
    Id:number,
    Name:string,
}
export type UserToGroupRelation = "Inviting"|"Member"|"Leader"

export interface UserGroupListResult{
    InvitingMe:UserGroupListResultItem[],
    MeIn:UserGroupListResultItem[],
    Others:UserGroupListResultItem[]
}
export interface UserGroupListResultItem{
    Id:number,
    Name:string,
    MemberCount:number
}

export interface UserGroupDetailResult{
    Id:number,
    Name:string,
    Owner?:string,
    CanEdit:boolean,
    CanInvite:boolean,
    IsMember:boolean
    Inviting:UserGroupDetailResultMemberItem[],
    FormalMembers:UserGroupDetailResultMemberItem[]
}
export interface UserGroupDetailResultMemberItem{
    Id:number,
    Type:UserToGroupRelation,
    Name:string
}