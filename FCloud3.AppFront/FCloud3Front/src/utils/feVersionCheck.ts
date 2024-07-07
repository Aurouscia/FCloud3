import { feVersion as verInCode } from '@/build/feVersion';
import { injectPop } from '@/provides';

export function useFeVersionChecker(){
    const pop = injectPop();
    const check = async()=>{
        const verInPublic = await(await fetch('/feVersion.txt', {cache: "no-store"})).text();
        console.log(`代码版本:${verInCode}\n最新版本:${verInPublic}`);
        return verInPublic === verInCode
    };
    const checkAndPop = ()=>{
        setTimeout(async()=>{
            try{
                if(!(await check())){
                    console.warn("版本检查：并非最新版")
                    pop.value?.show("客户端已更新，建议刷新浏览器获取最新版", "warning")
                }else{
                    console.log("版本检查：通过")
                }
            }
            catch{
                pop.value?.show("版本检查失败", "failed");
            }
        },1000)
    }

    return {check, checkAndPop}
}