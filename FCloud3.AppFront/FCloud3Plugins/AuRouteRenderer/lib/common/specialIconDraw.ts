export function drawAirport(ctx:CanvasRenderingContext2D, x:number, y:number, radius:number){
    const s = radius;
    const bodyWidthHalf = s/6
    const wingSpan = s*0.9
    const tailWingSpan = s*0.4
    const headTipY =  y-s*0.9
    const bodyLeftX = x-bodyWidthHalf
    const bodyRightX = x+bodyWidthHalf
    const headBottomY = y-s+bodyWidthHalf*2

    const wingRootUpperY = y-s/4
    const wingRootLowerY = y+s/6
    const wingTipUpperY = y+s/16
    const wingTipLowerY = y+s/4
    const wingTipLeftX = x-wingSpan
    const wingTipRightX = x+wingSpan

    const tailWingRootUpperY = y+s*0.6
    const tailWingTipLeftX = x-tailWingSpan
    const tailWingTipRightX = x+tailWingSpan
    const buttTipY = y+s*0.9

    ctx.beginPath()
    ctx.moveTo(x, headTipY)
    ctx.lineCap = 'round'
    ctx.lineJoin = 'round'
    ctx.quadraticCurveTo(bodyLeftX, headTipY, bodyLeftX, headBottomY)
    ctx.lineTo(bodyLeftX, wingRootUpperY)
    ctx.lineTo(wingTipLeftX, wingTipUpperY)
    ctx.lineTo(wingTipLeftX, wingTipLowerY)
    ctx.lineTo(bodyLeftX, wingRootLowerY)
    ctx.lineTo(bodyLeftX, tailWingRootUpperY)
    ctx.lineTo(tailWingTipLeftX, buttTipY)

    ctx.lineTo(tailWingTipRightX, buttTipY)
    ctx.lineTo(bodyRightX, tailWingRootUpperY)
    ctx.lineTo(bodyRightX, wingRootLowerY)
    ctx.lineTo(wingTipRightX, wingTipLowerY)
    ctx.lineTo(wingTipRightX, wingTipUpperY)
    ctx.lineTo(bodyRightX, wingRootUpperY)
    ctx.lineTo(bodyRightX, headBottomY)
    ctx.quadraticCurveTo(bodyRightX, headTipY, x, headTipY)
    ctx.stroke()
}