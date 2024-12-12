export const trigger = 'AuTimeOffset'

export interface TargetGroup{
    table:HTMLTableElement
    targets:Target[]
}
export interface Target{
    t:Date
    desc?:string
}