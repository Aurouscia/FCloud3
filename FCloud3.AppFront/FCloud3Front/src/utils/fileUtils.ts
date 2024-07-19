import { CSSProperties } from "vue";
import { Router } from "vue-router";

export type FileType = "image"|"video"|"audio"|"text"|"unknown";

export function getFileType(fileName:string):FileType{
    fileName = fileName.toLowerCase();
    if(imageFileExts.find(x=>fileName.endsWith(x))){
        return "image"
    }
    if(videoFileExts.find(x=>fileName.endsWith(x))){
        return "video"
    }
    if(textFileExts.find(x=>fileName.endsWith(x))){
        return "text"
    }
    if(audioFileExts.find(x=>fileName.endsWith(x))){
        return "audio"
    }
    return "unknown"
}
export function isImageFile(fileName:string):boolean{
    return getFileType(fileName)=="image"
}

export const imageFileExts=[
    '.png',
    '.jpg','.jpeg',
    '.gif','.webp',
    '.svg','.svgz',
    '.tif','.tiff','.pjp','.pjpeg',
    '.xbm','.ico','.jfif','.avif'
]
export const videoFileExts=[
    '.mp4','.webm'
]
export const textFileExts=[
    '.txt','.md','.json','.xml',
    '.html','.js','.css'
]
export const audioFileExts=[
    '.mp3','.ogg','.aac'
]

export function fileSizeStr(bytes:number){
    if(bytes<1000){
        return bytes+"B";
    }
    if(bytes<1000*1000){
        return Math.round(bytes/1000)+"K";
    }
    return (Math.round(bytes/(1000*100)))/10+"M"
}

export function getFileExt(str:string, truncate = true, withDot = false):string{
    var parts = str.split('.');
    if(parts.length==1){
        return "???";
    }
    var ext = parts[parts.length-1];
    if(truncate){
        if(ext.length>4){
        ext = ext.substring(0,4);
        }
    }
    if(withDot){
        return '.'+ext;
    }
    return ext;
}
export function fileNameWithoutExt(str:string):string{
    return str.replace(/\.[^/.]+$/, "")
}
export function getFileIconStyle(fileName:string):CSSProperties{
    var type = getFileType(fileName);
    if(type=='image'){
        return {backgroundColor:'#97C0D0'}
    }
    if(type=="video"){
        return {backgroundColor:'#D6ACC3'}
    }
    if(type=='text'){
        return {backgroundColor:'#96B23C'}
    }
    if(type=='audio'){
        return {backgroundColor:'#EFD67F'}
    }
    return {backgroundColor:'#AAAAAA'}
}

export function canDisplayAsImage(fileName:string, byteCount:number){
    return isImageFile(fileName) && byteCount < 1024 * 1024
}

export function fileLocationShow(path:string[]){
    if(!path || path.length==0){
        return "无归属"
    }
    return path.join('/')
}