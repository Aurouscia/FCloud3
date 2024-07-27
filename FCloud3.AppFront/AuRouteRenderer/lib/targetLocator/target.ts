import { ValidMarks } from "../common/marks"

export interface Target{
    element:HTMLTableElement
    rowFrom:number
    marks:ValidMarks[]
}