<script setup lang="ts">
import Pop from './components/Pop.vue';
import TopbarParent from './components/Topbar/TopbarParent.vue';
import NeedMemberWarning from './components/NeedMemberWarning.vue';
import { useProvidesSetup } from './provides';
import Wait from './components/Wait.vue';

const { pop, displayTopbar, needMemberWarning, wait, mainDivStyle} = useProvidesSetup();

</script>

<template>
  <Pop ref="pop"></Pop>
  <Wait ref="wait"></Wait>
  <NeedMemberWarning ref="needMemberWarning"></NeedMemberWarning>
  <TopbarParent v-if="displayTopbar"></TopbarParent>
  <div class="main" :style="mainDivStyle">
    <RouterView></RouterView>
  </div>
</template>

<style>
</style>