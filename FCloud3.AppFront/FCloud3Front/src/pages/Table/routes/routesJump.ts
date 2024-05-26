import { useRouter } from "vue-router"

export const tableEditRouteName = 'tableEdit';
export function useTableRoutesJump(){
    const router = useRouter();
    const jumpToFreeTableEdit = (id:number) => {
        router?.push({name:tableEditRouteName, params:{id:id}})
    }
    return { jumpToFreeTableEdit }
}