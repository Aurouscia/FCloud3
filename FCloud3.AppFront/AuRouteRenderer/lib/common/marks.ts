export type ValidMark = 'o'|'I'|' '
export const marksDefined:Record<string, ValidMark> = {
    sta:'o',
    line:'I',
    empty:' '
}
export const seperator = ';'
export const configSeperator = '_'
export const configKvSeperator = ':'