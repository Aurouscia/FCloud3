export const trigger = 'AuTimeOffset'

export interface TargetGroup{
    table:HTMLTableElement
    targets:Target[]
}
export interface Target{
    t:Date
    specifyTimeOfDay:boolean
    desc?:string
}