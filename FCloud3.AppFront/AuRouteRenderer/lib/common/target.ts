import { ValidMarks } from "./marks"

export interface Target{
    element:HTMLTableElement
    rowFrom:number
    marks:ValidMarks[]
    cvs?:HTMLCanvasElement
}