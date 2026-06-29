<script setup lang="ts">
import { computed, inject, onMounted, onUnmounted, ref } from 'vue';
import { useRoute } from 'vue-router';
import { storeToRefs } from 'pinia';
import { HttpClient } from '@/utils/com/httpClient';
import { IdentityInfoProvider, useIdentityInfoStore } from '@/utils/globalStores/identityInfo';
import { injectIdentityInfoProvider } from '@/provides';
import { useIdentityRoutesJump } from './routes/routesJump';
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';

export interface F3SsoIssuerAudienceOptions {
    id: string;
    displayName: string;
    origin: string;
    avatar: string;
    requireLevel: number;
}

export interface F3SsoIssuerOptions {
    id: string;
    enabled: boolean;
    audiences: F3SsoIssuerAudienceOptions[];
}

export interface F3SsoAudienceIssuerOptions {
    id: string;
    displayName: string;
    origin: string;
    avatar: string;
    clientId: string;
}

export interface F3SsoAudienceOptions {
    id: string;
    enabled: boolean;
    issuers: F3SsoAudienceIssuerOptions[];
}

export interface F3SsoCodeResponse {
    code: string;
}

async function getF3SsoConfig(http: HttpClient): Promise<F3SsoIssuerOptions | undefined> {
    return await http.requestRaw<F3SsoIssuerOptions>('/f3sso/iss/config', undefined, ['id', 'enabled', 'audiences']);
}

async function requestF3SsoCode(http: HttpClient, clientId: string): Promise<string | undefined> {
    const res = await http.requestRaw<F3SsoCodeResponse>('/f3sso/iss', { clientId }, ['code']);
    return res?.code;
}

const { jumpToLogin } = useIdentityRoutesJump();
const { iden } = storeToRefs(useIdentityInfoStore());
const route = useRoute();
let identityInfoProvider: IdentityInfoProvider;
let httpClient: HttpClient;
const loaded = ref<boolean>(false);
const config = ref<F3SsoIssuerOptions>();
const clientId = route.query.clientId?.toString() ?? '';
const selectedAudience = computed<F3SsoIssuerAudienceOptions | undefined>(() => {
    return config.value?.audiences.find(c => c.id === clientId);
});
const levelSufficient = computed<boolean>(() => {
    return iden.value.Type >= (selectedAudience.value?.requireLevel ?? 255);
});

async function authorize() {
    if (!config.value?.id) {
        return;
    }
    if (!levelSufficient.value) {
        window.alert('该账号等级不足以登录目标应用');
        return;
    }
    const code = await requestF3SsoCode(httpClient, clientId);
    if (code && selectedAudience.value?.origin) {
        const origin = selectedAudience.value.origin.replace(/\/$/, '');
        const issuerId = encodeURIComponent(config.value.id);
        window.location.href = `${origin}/f3sso/aud/validate?code=${encodeURIComponent(code)}&issuerId=${issuerId}`;
    }
}

onMounted(async () => {
    setTitleTo('SSO 授权');
    identityInfoProvider = injectIdentityInfoProvider();
    httpClient = inject('http') as HttpClient;
    await identityInfoProvider.getIdentityInfo(true);
    config.value = await getF3SsoConfig(httpClient);
    loaded.value = true;
});
onUnmounted(() => {
    recoverTitle();
});
</script>

<template>
    <div class="ssoAuthorize">
        <div v-if="!loaded" class="centerBox">加载中...</div>
        <div v-else-if="iden.Id === 0" class="centerBox">
            <div>请先登录本站账号</div>
            <button @click="jumpToLogin(route.fullPath)">去登录</button>
        </div>
        <div v-else-if="!config?.enabled" class="centerBox">
            SSO 服务未启用
        </div>
        <div v-else-if="!selectedAudience" class="centerBox">
            配置异常，无法登录，请联系网站管理员
        </div>
        <div v-else-if="!levelSufficient" class="centerBox">
            该账号等级不足以登录目标应用
        </div>
        <div v-else class="centerBox">
            <img v-if="selectedAudience.avatar" :src="selectedAudience.avatar" alt="应用图标" class="avatar"/>
            <div>确认授权{{ selectedAudience.displayName }}登录吗？</div>
            <button @click="authorize">确认授权</button>
        </div>
    </div>
</template>

<style scoped lang="scss">
.ssoAuthorize {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 60vh;
}
.centerBox {
    text-align: center;
    font-size: large;
    color: gray;
    button {
        margin-top: 20px;
    }
}
.avatar {
    width: 80px;
    height: 80px;
    object-fit: contain;
    margin-bottom: 10px;
}
</style>
