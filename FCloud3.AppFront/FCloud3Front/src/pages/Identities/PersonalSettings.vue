<script setup lang="ts">
import { onMounted, ref,Ref, watch } from 'vue';
import Pop from '@/components/Pop.vue';
import {User, UserType} from '@/models/identities/user';
import { Api } from '@/utils/com/api';
import SwitchingTabs from '@/components/SwitchingTabs.vue';
import { injectApi, injectHttp, injectIdentityInfoProvider, injectPop } from '@/provides';
import { random } from 'lodash';
import Search from '@/components/Search.vue';
import jsfd from 'js-file-download';
import { truncate } from 'lodash';
import { timeReadable } from '@/utils/timeStamp';
import { useNotifCountStore } from '@/utils/globalStores/notifCount';
import { useRouter } from 'vue-router';

const user = ref<User>();
var api:Api;
var pop:Ref<InstanceType<typeof Pop>>
const identityInfoProvider = injectIdentityInfoProvider()
const httpClient = injectHttp()
const notifCountStore = useNotifCountStore()
const router = useRouter()

const props = defineProps<{
    user:User
}>();
watch(props, (newVal)=>{
    user.value = newVal.user;
})

const editedNameOrPwd = ref<boolean>(false)
async function editUserInfo(){
    if(user.value){
        if(user.value.Name){
            if(user.value.Pwd){
                if(!pwdRepeat.value){
                    pop.value.show("请再次输入密码", "failed");
                    return;
                }
                if(user.value.Pwd != pwdRepeat.value){
                    pop.value.show("两次密码不一致", "failed");
                    return;
                }
            }
            const resp = await api.identites.user.editExe(user.value);
            if(resp){
                if(editedNameOrPwd.value){
                    editedNameOrPwd.value = false
                    httpClient.clearToken();
                    identityInfoProvider.clearCache();
                    notifCountStore.enforceRefresh()
                    pop.value.show("立即重新登录", "warning");
                    router.push({name:'login'})
                }
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

async function exportMyWikis() {
    const data = await api.split.wikiImportExport.exportMyWikis()
    if(data){
        const nameTrun = truncate(user.value?.Name||'', {length:10, omission:''})
        jsfd(data, `${nameTrun}_我的词条_${timeReadable('ymdhm')}.zip`)
        pop.value.show('获取成功，已下载', 'success')
    }
}
async function exportAllWikis() {
    const data = await api.split.wikiImportExport.exportAllWikis()
    if(data){
        jsfd(data, `所有词条_${timeReadable('ymdhm')}.zip`)
        pop.value.show('获取成功，已下载', 'success')
    }
}

const pwdRepeat = ref<string>();
const isSuperAdmin = ref(false)
const allowExportAll = import.meta.env.VITE_AllowExportAllWikis === 'true'
onMounted(async()=>{
    pop = injectPop();
    api = injectApi();
    user.value = props.user
    const info = await identityInfoProvider.getIdentityInfo()
    const type = info.Type ?? UserType.Tourist
    isSuperAdmin.value = type >= UserType.SuperAdmin
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
                        <input v-model="user.Name" @input="editedNameOrPwd=true"/>
                    </td>
                </tr>
                <tr>
                    <td>密码</td>
                    <td>
                        <input v-model="user.Pwd" @input="editedNameOrPwd=true" type="password" autocomplete="new-password"/>
                    </td>
                </tr>
                <tr v-if="user.Pwd">
                    <td>再次<br/>输入</td>
                    <td>
                        <input v-model="pwdRepeat" type="password" autocomplete="new-password"/>
                    </td>
                </tr>
                <tr>
                    <td>个人<br/>简介</td>
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
        <h1>导出所有词条</h1>
        <div class="section">
            <button @click="exportMyWikis" class="wikiExportBtn">导出本账号所有词条</button>
        </div>
        <div class="section" v-if="isSuperAdmin && allowExportAll">
            <button @click="exportAllWikis" class="wikiExportBtn">导出本站所有词条（仅限超管）</button>
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
.wikiExportBtn{
    display: block;
    margin: auto;
}
input, textarea{
    width: 180px;
}
</style>