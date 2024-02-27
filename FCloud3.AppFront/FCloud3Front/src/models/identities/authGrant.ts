export interface AuthGrant{
    Id:number
    ToId:number
    To:AuthGrantTo
    OnId:number
    On:AuthGrantOn
    IsReject:boolean
}

export interface AuthGrantViewModel extends AuthGrant{
    ToName:string
    CreatorName:string
}

export type AuthGrantOn = 0|1|2;
export type AuthGrantTo = 0|1|2|3;

export type AuthGrantOnText = "None"|"Wiki"|"Dir"
export type AuthGrantToText = "None"|"User"|"UserGroup"|"EveryOne"

const authGrantOnList:AuthGrantOnText[] = ["None","Wiki","Dir"];
const authGrantToList:AuthGrantToText[] = ["None","User","UserGroup","EveryOne"];
export function authGrantOn(typeStr:AuthGrantOnText){
    return authGrantOnList.indexOf(typeStr) as AuthGrantOn;
}
export function authGrantTo(typeStr:AuthGrantToText){
    return authGrantToList.indexOf(typeStr) as AuthGrantTo;
}