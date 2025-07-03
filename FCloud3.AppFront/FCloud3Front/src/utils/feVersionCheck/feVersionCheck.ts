import { appVersionCheck } from '@aurouscia/vite-app-version/check';
import { injectPop } from '@/provides';

export function useFeVersionChecker(){
    const pop = injectPop();
    const check = async()=>{
        const obj = JSON.parse(__VERSION_CONFIG__)
        return await appVersionCheck(obj, true)
    };
    const checkAndPop = ()=>{
        setTimeout(async()=>{
            try{
                if(!(await check())){
                    console.warn("版本检查：并非最新版")
                    pop.value?.show("客户端已更新，刷新页面获取最新版", "failed")
                }else{
                    console.log("版本检查：通过")
                }
            }
            catch(err){
                pop.value?.show("版本检查失败", "failed");
                throw err
            }
        },100)
    }
    return {check, checkAndPop}
}