export const staMark = 'o'
export const lineMark = 'T'
export const emptyMark = '_'
export const transLeftMark = '/'
export const transRightMark = '\\'
export const waterMark = '~'
export const isTransMark=(s?:string) => s===transLeftMark || s===transRightMark; 
export const needFillLine=(s?:string) => s===lineMark || s===staMark || s===waterMark
export type ValidMark = typeof staMark|typeof lineMark|typeof emptyMark|typeof transLeftMark|typeof transRightMark|typeof waterMark
export const marksDefined:Record<string, ValidMark> = {
    sta:staMark,
    line:lineMark,
    empty:emptyMark,
    transLeft:transLeftMark,
    transRight:transRightMark,
    water:waterMark
}
export const seperator = ';'
export const configSeperator = '_'
export const configKvSeperator = ':'