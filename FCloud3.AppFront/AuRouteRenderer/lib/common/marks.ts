export type ValidMark = 'o'|'I'|'_'
export const marksDefined:Record<string, ValidMark> = {
    sta:'o',
    line:'I',
    empty:'_'
}
export const seperator = ';'
export const configSeperator = '_'
export const configKvSeperator = ':'