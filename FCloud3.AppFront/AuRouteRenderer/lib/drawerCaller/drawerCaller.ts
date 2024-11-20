import { hillColor, waterColor } from "../common/consts";
import { branchBothMark, branchLowerMark, branchUpperMark, emptyMark, hillMark, isTransMark, isTurnMark, needButt, needFillLine,
    noActiveLink,
    staMark, transLeftMark, transRightMark, turnBottomMark, turnSpanMark, turnTopMark, ValidMark, waterMark } from "../common/marks";
import { gridNeighbor, gridNeighborActiveLink, Target } from "../common/target";
import { Drawer, DrawIconType, DrawLineConfig, DrawLineType, DrawStationType, DrawTurnType } from "../drawer/drawer";

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

    type ConnectInfo = {top?:boolean, bottom?:boolean, topBias?:number, bottomBias?:number}
    const connectInfo:ConnectInfo[][] = []
    for(let i=0; i<t.grid.length; i++){
        connectInfo[i] = []
        for(let j=0; j<t.gridColCount; j++){
            connectInfo[i][j] = {}
        }
    }
    enumerateGrid((x,y,mark)=>{
        if(mark===waterMark){
            drawer.drawTerrain({x,y}, waterColor)
        }
        else if(mark===hillMark){
            drawer.drawTerrain({x,y}, hillColor)
        }
    })
    const drawAllLines = (color:string, lineWidthRatio?:number, recordConnectInfo?:boolean)=>{
        enumerateGrid((x,y,mark)=>{
            if(needFillLine(mark)){
                const type = staLineType(t.grid, y, x)
                if(type){
                    const config:DrawLineConfig = {lineWidthRatio}
                    drawer.drawLine({x,y}, color, type, config)

                    if(recordConnectInfo){
                        connectInfo[y][x].bottom = true
                        connectInfo[y][x].top = true
                    }
                }
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

                    if(recordConnectInfo){
                        if(typeof param.topBias == 'number')
                        {
                            connectInfo[y][x+param.topBias].top = true
                            connectInfo[y][x].topBias = param.topBias
                        }
                        if(typeof param.bottomBias == 'number'){
                            connectInfo[y][x+param.bottomBias].bottom = true
                            connectInfo[y][x].bottomBias = param.bottomBias
                        }
                    }
                }
            }
        })
    }
    const drawAllTurns = (color:string, lineWidthRatio?:number)=>{
        let rowIdx = 0
        t.grid.forEach(row=>{
            let isInTurn:typeof turnBottomMark|typeof turnTopMark|null = null
            let turnLeftX = 0;
            let turnWidth = 0;
            let cursor = 0;
            const rowExtended = [...row, 'T']
            rowExtended.forEach(m=>{
                if(m==turnTopMark || m==turnBottomMark || m===turnSpanMark){
                    if(!isInTurn && isTurnMark(m)){
                        isInTurn = m
                        turnLeftX = cursor
                    }else if(m===isInTurn || m===turnSpanMark){
                        turnWidth = cursor - turnLeftX + 1
                    }else{
                        const type:DrawTurnType = isInTurn === turnBottomMark?'bottom':'top'
                        drawer.drawTurn({x:turnLeftX, y:rowIdx}, color, type, {lineWidthRatio, widthBlocks:turnWidth})
                        isInTurn = m
                        turnLeftX = cursor
                    }
                }else if(isInTurn){
                    const type:DrawTurnType = isInTurn === turnBottomMark?'bottom':'top'
                    drawer.drawTurn({x:turnLeftX, y:rowIdx}, color, type, {lineWidthRatio, widthBlocks:turnWidth})
                    isInTurn = null
                }
                cursor++;
            })
            rowIdx++;
        })
    }
    const drawAllSta = (noStroke?:boolean, radiusRatio?:number)=>{
        enumerateGrid((x,y,mark)=>{
            if(mark === staMark){
                const isExchangeRow = t.annotations[y]?.some(x => !isIconCall(x))
                const type:DrawStationType = isExchangeRow ? 'cross' : 'single'
                drawer.drawStation({x,y}, color, type, {noStroke, radiusRatio})
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
    const drawButts = (color:string, lineWidthRatio?:number)=>{
        enumerateGrid((x,y,mark)=>{
            if(needFillLine(mark) && needButt(mark)){
                const topConn = gridNeighbor<ConnectInfo>(connectInfo, x, y, "middle", "up")?.bottom
                const bottomConn = gridNeighbor<ConnectInfo>(connectInfo, x, y, "middle", "down")?.top
                if(!topConn){
                    drawer.drawButt({x,y}, color, 'up', {lineWidthRatio})
                }else if(!bottomConn){
                    drawer.drawButt({x,y}, color, 'down', {lineWidthRatio})
                }
            }
            else if(isTransMark(mark)){
                const info = connectInfo[y][x]
                const topX = x + (info.topBias||0)
                const bottomX = x + (info.bottomBias||0)
                let topConn:boolean
                if(y>0){
                    topConn = !!connectInfo[y-1][topX].bottom
                }else{
                    topConn = false
                }
                let bottomConn:boolean
                if(y<connectInfo.length-1){
                    bottomConn = !!connectInfo[y+1][bottomX].top
                }else{
                    bottomConn = false
                }
                if(!topConn){
                    if(needButt(gridNeighbor<string>(t.grid, x, y, "middle", "up")))
                        drawer.drawButt({x:topX,y:y-1}, color, 'up', {lineWidthRatio})
                }
                if(!bottomConn){
                    if(needButt(gridNeighbor<string>(t.grid, x, y, "middle", "down")))
                        drawer.drawButt({x:bottomX,y:y+1}, color, 'down', {lineWidthRatio})
                }
            }
        })
    }
    drawAllLines('white', 1.4, true)
    drawAllTurns('white', 1.4)
    drawButts('white', 0.6)
    drawBranches(color)
    drawAllSta(true, 1.4)
    drawAllLines(color)
    drawAllTurns(color)
    drawAllSta()
    drawButts(color, 0)

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
    if(!upNei || upNei===emptyMark || isTransMark(upNei) || noActiveLink(upNei)){
        topConn = false
    }
    if(!downNei || downNei===emptyMark || isTransMark(downNei) || noActiveLink(downNei)){
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

function isIconCall(anno:string):boolean{
    return anno.startsWith('_')
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