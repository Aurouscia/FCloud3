export interface Target{
    element:HTMLTableElement
    rowFrom:number
    cells:string[]
    grid:string[][]
    annotations:string[][]
    config:TargetConfig
    cvs?:HTMLCanvasElement
}
export type TargetConfig = {
    c:string
} & Record<string,string|null|undefined>
export const targetConfigDefault:TargetConfig = {
    c:"#ff0000"
}