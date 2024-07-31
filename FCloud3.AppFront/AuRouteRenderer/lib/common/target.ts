export interface Target{
    element:HTMLTableElement
    rowFrom:number
    cells:string[]
    grid:string[][]
    annotations:string[][]
    cvs?:HTMLCanvasElement
}