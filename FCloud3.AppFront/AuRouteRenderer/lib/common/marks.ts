export const staMark = 'o'
export const lineMark = 'T'
export const emptyMark = '_'
export const transLeftMark = '/'
export const transRightMark = '\\'
export const waterMark = '~'
export const branchUpperMark = 'p'
export const branchLowerMark = 'b'
export const branchBothMark = "B"
export const isTransMark=(s?:string) => s===transLeftMark || s===transRightMark; 
const needFillLineMarks = [lineMark, staMark, waterMark, branchBothMark, branchLowerMark, branchUpperMark]
export const needFillLine=(s?:string) => s && needFillLineMarks.includes(s)
export type ValidMark = typeof staMark|typeof lineMark|typeof emptyMark|typeof transLeftMark|
    typeof transRightMark|typeof waterMark|typeof branchUpperMark|typeof branchLowerMark|typeof branchBothMark
export const marksDefined:Record<string, ValidMark> = {
    sta:staMark,
    line:lineMark,
    empty:emptyMark,
    transLeft:transLeftMark,
    transRight:transRightMark,
    water:waterMark,
    branchUpper: branchUpperMark,
    branchLower: branchLowerMark,
    branchBoth: branchBothMark
}
export const seperator = ';'
export const configSeperator = '_'
export const configKvSeperator = ':'