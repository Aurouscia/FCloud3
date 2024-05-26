import { useRouter } from "vue-router"

export const textSectionEditRouteName = 'textSectionEdit';
export function useTextSectionRoutesJump(){
    const router = useRouter();
    const jumpToTextSectionEdit = (id:number) => {
        router?.push({name:textSectionEditRouteName, params:{id:id}})
    }
    return { jumpToTextSectionEdit }
}