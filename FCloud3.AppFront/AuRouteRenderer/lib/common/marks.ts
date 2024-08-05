export const staMark = 'o'
export const lineMark = 'T'
export const emptyMark = '_'
export const transLeftMark = '/'
export const transRightMark = '\\'
export type ValidMark = typeof staMark|typeof lineMark|typeof emptyMark|typeof transLeftMark|typeof transRightMark
export const marksDefined:Record<string, ValidMark> = {
    sta:staMark,
    line:lineMark,
    empty:emptyMark,
    transLeft:transLeftMark,
    transRight:transRightMark
}
export const seperator = ';'
export const configSeperator = '_'
export const configKvSeperator = ':'