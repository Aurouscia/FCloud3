<script setup lang="ts">
import { onMounted, ref,Ref, watch } from 'vue';
import Pop from '@/components/Pop.vue';
import {User} from '@/models/identities/user';
import { Api } from '@/utils/com/api';
import SwitchingTabs from '@/components/SwitchingTabs.vue';
import { injectApi, injectPop } from '@/provides';
import { random } from 'lodash';
import Search from '@/components/Search.vue';

const user = ref<User>();
var api:Api;
var pop:Ref<InstanceType<typeof Pop>>

const props = defineProps<{
    user:User
}>();
watch(props, (newVal)=>{
    user.value = newVal.user;
})

async function editUserInfo(){
    if(user.value){
        if(user.value.Name){
            const resp = await api.identites.user.editExe(user.value);
            if(resp){
                emits('requireReload')
            }
        }
        else{
            pop.value.show("请填写用户名","failed");
        }
    }
}

let uploadingAvatar:File|null|undefined
const avatarFileInput = ref<HTMLInputElement>();
async function avatarFileInputChange(){
    if(avatarFileInput.value?.files){
        uploadingAvatar = avatarFileInput.value.files[0];
    }else{
        uploadingAvatar = undefined;
    }
}
async function uploadAvatar() {
    if(!uploadingAvatar){
        pop.value.show("请先选择文件", "failed");
        return;
    }
    if(!user.value){
        pop.value.show("请先登录", "failed");
        return;
    }
    const name = avatarMaterialName();
    if(!name){return;}
    const matId = await api.files.material.add(name, "", uploadingAvatar);
    if(matId){
        const replaceAvtResp = await api.identites.user.replaceAvatar(user.value.Id, matId);
        if(replaceAvtResp){
            emits('requireReload')
        }
    }
}
function avatarMaterialName(){
    return "头像_" + random(100000000,999999999);
}

async function setAvatarToSearchRes(_value:string, id:number) {
    if(!user.value){return;}
    const resp = await api.identites.user.replaceAvatar(user.value.Id, id);
    if(resp){
        emits('requireReload');
    }
}

const emits = defineEmits<{
    (e:'requireReload'):void
}>()

onMounted(async()=>{
    pop = injectPop();
    api = injectApi();
    user.value = props.user
})
</script>

<template>
    <div v-if="user">
        <h1>编辑个人信息</h1>
        <div class="section">
            <table><tbody>
                <tr>
                    <td>昵称</td>
                    <td>
                        <input v-model="user.Name"/>
                    </td>
                </tr>
                <tr>
                    <td>密码</td>
                    <td>
                        <input v-model="user.Pwd"/>
                    </td>
                </tr>
                <tr>
                    <td>个人简介</td>
                    <td>
                        <textarea class="infoEditTextarea" v-model="user.Desc" rows="3"></textarea>
                    </td>
                </tr>
                <tr class="noneBackground">
                    <td colspan="2">
                        <button @click="editUserInfo">保存</button>
                    </td>
                </tr>
            </tbody></table>
        </div>
        <h1>设置头像</h1>
        <div class="section">
            <div class="currentAvatar">
                <img :src="user.AvatarSrc"/>
            </div>
            <SwitchingTabs :texts="['上传图片','选择素材']">
                <div style="text-align: center;">
                    <input type="file" ref="avatarFileInput" @change="avatarFileInputChange"/>
                    <button @click="uploadAvatar">确定</button>
                </div>
                <div>
                    <Search :source="api.etc.quickSearch.material" @done="setAvatarToSearchRes"></Search>
                </div>
            </SwitchingTabs>
        </div>
    </div>
</template>

<style scoped lang="scss">
.currentAvatar{
    display: flex;
    justify-content: center;
    align-items: center;
    margin: 10px 0px 10px 0px;
    img{
        width: 100px;
        height: 100px;
        object-fit: contain;
        border-radius: 50%;
        border: 2px solid #eee
    }
}
</style>