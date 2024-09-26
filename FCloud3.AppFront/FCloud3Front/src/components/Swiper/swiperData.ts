export interface SwiperData{
    items: SwiperDataItem[]
    width?: number
    height?: number
}
export const defaultWidth = 500
export const defaultHeight = 300
export interface SwiperDataItem{
    link:string
    imgUrl:string
    title:string
    desc:string
}