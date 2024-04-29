export function rateColor(rate:number, pos: number=-1) {
    if (pos==-1 && rate==0){
        return "#666"
    }
    if (pos >= rate) {
        return "#ccc"
    }
    if (rate <= 2){
        return "red"
    }
    if (rate <= 4){
        return "orange"
    }
    if (rate <= 6){
        return "cornflowerblue"
    }
    if (rate <= 8){
        return "olivedrab"
    }
    return "green"
}
export function rateText(rate:number){
    if (rate == 0){
        return "评分"
    }
    if (rate <= 2){
        return "劣质"
    }
    if (rate <= 4){
        return "差劲"
    }
    if (rate <= 6){
        return "一般"
    }
    if (rate <= 8){
        return "很好"
    }
    return "超棒"
}