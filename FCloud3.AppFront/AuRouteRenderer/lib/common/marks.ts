export type ValidMark = 'o'|'l'|'_'|'/'|'\\'
export const marksDefined:Record<string, ValidMark> = {
    sta:'o',
    line:'l',
    empty:'_',
    transLeft:'/',
    transRight:'\\'
}
export const seperator = ';'
export const configSeperator = '_'
export const configKvSeperator = ':'