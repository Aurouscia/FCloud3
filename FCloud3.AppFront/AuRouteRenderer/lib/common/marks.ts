export const staMark = 'o'
export const lineMark = 'T'
export const emptyMark = '_'
export const transLeftMark = '/'
export const transRightMark = '\\'
export const waterMark = '~'
export const branchUpperMark = 'b'
export const branchLowerMark = 'p'
export const branchBothMark = "B"
export const turnTopMark = 'n'
export const turnBottomMark = 'u'
export const turnSpanMark = '-'
export const isTransMark=(s?:string) => s===transLeftMark || s===transRightMark; 
export const isTurnMark=(s?:string) => s===turnBottomMark || s===turnTopMark;
const needFillLineMarks = [lineMark, staMark, waterMark, branchBothMark, branchLowerMark, branchUpperMark, turnTopMark, turnBottomMark]
const noButtMarks = [turnTopMark, turnBottomMark, turnSpanMark, staMark, waterMark]
const noActiveLinkMarks = [turnSpanMark]
export const needFillLine=(s?:string) => s && needFillLineMarks.includes(s)
export const needButt=(s?:string) => s && !noButtMarks.includes(s)
export const noActiveLink=(s?:string) => !s || noActiveLinkMarks.includes(s)
export type ValidMark = typeof staMark|typeof lineMark|typeof emptyMark|typeof transLeftMark|
    typeof transRightMark|typeof waterMark|typeof branchUpperMark|typeof branchLowerMark|typeof branchBothMark|
    typeof turnTopMark|typeof turnBottomMark|typeof turnSpanMark
export const marksDefined:Record<string, ValidMark> = {
    sta:staMark,
    line:lineMark,
    empty:emptyMark,
    transLeft:transLeftMark,
    transRight:transRightMark,
    water:waterMark,
    branchUpper: branchUpperMark,
    branchLower: branchLowerMark,
    branchBoth: branchBothMark,
    turnTopMark: turnTopMark,
    turnBottomMark: turnBottomMark,
    turnSpan: turnSpanMark
}
export const seperator = ';'
export const configSeperator = '_'
export const configKvSeperator = ':'