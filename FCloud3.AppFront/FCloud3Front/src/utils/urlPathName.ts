// import _ from 'lodash';
// import pinyin from 'pinyin'

// export function toUrlPathName(name:string){
//     if (!name.trim()){
//         return;
//     }
//     const clusters:Array<string> = []
//     var buildingCluster = "";
//     for(var i = 0; i < name.length; i++)
//     {
//         const c = name[i];
//         if (c >= '\u4e00' && c<='\u9fa5' )
//         {
//             if (buildingCluster.length > 0)
//             {
//                 clusters.push(buildingCluster);
//                 buildingCluster="";
//             }
//             clusters.push(pinyin(c,{style:0,segment:false})[0][0]);
//         }
//         else if (isUrlValidChar(c))
//         {
//             buildingCluster+=c;
//         }
//     }
//     return _.join(clusters,'-')
// }

// function isUrlValidChar(c:string){
//     return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c=='-';
// }