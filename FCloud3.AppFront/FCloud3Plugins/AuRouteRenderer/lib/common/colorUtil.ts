import Color from 'color'
export function autoTextColor(bgColor:string){
    let bg
    try{
        bg = Color(bgColor)
    }
    catch{
        bg = Color.rgb(255,255,255)
    }
    if(bg.darken(0.2).isLight()){
        return 'black'
    }else{
        return 'white'
    }
}