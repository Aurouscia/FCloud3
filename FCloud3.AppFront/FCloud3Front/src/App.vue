<script setup lang="ts">
import { provide, ref} from 'vue';
import { HttpCallBack, HttpClient } from './utils/httpClient';
import { IdentityInfoProvider } from './utils/userInfo';
import Pop from './components/Pop.vue';
import Topbar from './components/Topbar.vue';
import { useRouter } from 'vue-router';
import { Api } from './utils/api';

const pop = ref<InstanceType<typeof Pop> | null>(null);
provide('pop', pop)
const httpCallBack:HttpCallBack = (result,msg)=>{
  if(result=='ok'){pop.value?.show(msg,'success')}
  else if(result=='err'){pop.value?.show(msg,'failed')}
  else if(result=='warn'){pop.value?.show(msg,'warning')}
}

const httpClient = new HttpClient(httpCallBack)
provide('http', httpClient)
const api = new Api(httpClient);
provide('api',api)
provide('userInfo', new IdentityInfoProvider(api))

const displayTopbar = ref<boolean>(true);
provide('hideTopbar',()=>{displayTopbar.value=false})

const router = useRouter();
router.afterEach(()=>{
  displayTopbar.value = true;
})
</script>

<template>
  <Pop ref="pop"></Pop>
  <Topbar v-if="displayTopbar"></Topbar>
  <div class="main">
    <RouterView></RouterView>
  </div>
</template>

<style>
</style>