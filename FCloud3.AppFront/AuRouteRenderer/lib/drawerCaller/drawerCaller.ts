import { branchBothMark, branchLowerMark, branchUpperMark, emptyMark, isTransMark, needFillLine,
    staMark, transLeftMark, transRightMark, ValidMark, waterMark } from "../common/marks";
import { gridNeighbor, gridNeighborActiveLink, Target } from "../common/target";
import { Drawer, DrawIconType, DrawLineConfig, DrawLineType } from "../drawer/drawer";

export function callDrawer(t:Target, drawer:Drawer){
    const color = t.config.c
    const enumerateGrid = (cb:(x:number,y:number,mark:ValidMark)=>void)=>{
        for(let y=0;y<t.grid.length;y++){
            const gridRow = t.grid[y]
            for(let x=0;x<gridRow.length;x++){
                const mark = gridRow[x] as ValidMark
                cb(x,y,mark)
            }
        }
    }
    const enumerateAnno = (cb:(x:number,y:number,anno:string,isLast:boolean)=>void)=>{
        for(let y=0;y<t.annotations.length;y++){
            const annoRow = t.annotations[y]
            for(let x=0;x<annoRow.length;x++){
                const anno = annoRow[x] as string
                const isLast = x === annoRow.length-1
                cb(x,y,anno,isLast)
            }
        }
    }

    enumerateGrid((x,y,mark)=>{
        if(mark===waterMark){
            drawer.drawRiver({x,y})
        }
    })
    const drawAllLines = (color:string, lineWidthRatio?:number)=>{
        enumerateGrid((x,y,mark)=>{
            if(needFillLine(mark)){
                const type = staLineType(t.grid, y, x)
                if(type)
                    drawer.drawLine({x,y}, color, type, {lineWidthRatio})
            }else if(isTransMark(mark)){
                const param = transLineType(t.grid, y, x)
                if(param){
                    drawer.drawLine({x,y}, color, param.type, {
                        topBias:param.topBias,
                        bottomBias:param.bottomBias,
                        topShrink: param.topShrink,
                        bottomShrink:param.bottomShrink,
                        lineWidthRatio
                    })
                }
            }
        })
    }
    const drawAllSta = (noStroke?:boolean, radiusRatio?:number)=>{
        enumerateGrid((x,y,mark)=>{
            if(mark ==='o'){
                drawer.drawStation({x,y}, color, "single", {noStroke, radiusRatio})
            }
        })
    }
    const drawBranches = (color:string, lineWidthRatio?:number)=>{
        enumerateGrid((x,y,m)=>{
            if(m==branchUpperMark || m==branchBothMark){
                drawer.drawBranch({x,y}, color, 'upper',{lineWidthRatio})
            }
            if(m==branchLowerMark || m==branchBothMark){
                drawer.drawBranch({x,y}, color, 'lower',{lineWidthRatio})
            }
        })
    }
    drawAllLines('white', 1.4)
    drawBranches(color)
    drawAllSta(true, 1.4)
    drawAllLines(color)
    drawAllSta()

    enumerateAnno((x,y,anno,isLast)=>{
        const icon = readAnnoAsIcon(anno);
        const baseX = t.gridTrimmedLengths[y]
        const isOdd = !(x % 2)
        const biasType:DrawIconType = isOdd ? (isLast ? 'middle':'upper') : 'lower'
        const realX = Math.floor(x/2)
        if(icon){
            drawer.drawIcon({x:baseX+realX, y}, icon.bgColor, icon.text, biasType)
        }
    })
}


function staLineType(grid:string[][], y:number, x:number):DrawLineType|undefined{
    let topConn = true;
    let bottomConn = true;
    const upNei = gridNeighbor(grid, x, y, "middle", "up")
    const downNei = gridNeighbor(grid, x, y, "middle", "down")
    if(!upNei || upNei===emptyMark || isTransMark(upNei)){
        topConn = false
    }
    if(!downNei || downNei===emptyMark || isTransMark(downNei)){
        bottomConn = false
    }
    if(topConn){
        if(bottomConn)
            return 'regular'
        return 'endBottom'
    }
    if(bottomConn)
        return 'endTop'
    return undefined
}

function transLineType(grid:string[][], y:number, x:number)
        :(DrawLineConfig & {type:DrawLineType})|undefined{
    const trans = grid[y][x]
    if(trans===transLeftMark){
        let topBias = 0
        let bottomBias = 0
        let topShrink = false
        if(gridNeighborActiveLink(grid, x, y, "right", "up") && !gridNeighborActiveLink(grid, x, y, "middle", "up")){
            topBias = 1
        }
        if(gridNeighborActiveLink(grid, x, y, "left", "down") && !gridNeighborActiveLink(grid, x, y, "middle", "down")){
            bottomBias = -1
        }
        if(topBias == 0 && bottomBias == -1 && gridNeighbor(grid,x,y,"left","middle") === staMark){
            topShrink = true
        }
        return {type:'trans', topBias, bottomBias, topShrink}
    }
    if(trans===transRightMark){
        let topBias = 0
        let bottomBias = 0
        let bottomShrink = false
        if(gridNeighborActiveLink(grid, x, y, "left", "up") && !gridNeighborActiveLink(grid, x, y, "middle", "up")){
            topBias = -1
        }
        if(gridNeighborActiveLink(grid, x, y, "right", "down") && !gridNeighborActiveLink(grid, x, y, "middle", "down")){
            bottomBias = 1
        }
        if(topBias == -1 && bottomBias === 0 && gridNeighbor(grid,x,y,"left","middle") === staMark){
            bottomShrink = true
        }
        return {type:'trans', topBias, bottomBias, bottomShrink}
    }
    return undefined
}

function readAnnoAsIcon(anno:string):{text:string, bgColor:string}|undefined{
    const isIconFixed = /^_.*?_$/.test(anno)
    if(isIconFixed){
        return {
            text: anno, bgColor: '#999'
        }
    }
    const isIconWithParam = /.*\(.*?\)$/.test(anno)
    if(isIconWithParam){
        const firstB = anno.indexOf('(')
        const secondB = anno.indexOf(')')
        const text = anno.substring(0, firstB)
        let bgColor = anno.substring(firstB+1, secondB)
        bgColor = bgColor || '#999'
        return{
            text, bgColor
        }
    }
}