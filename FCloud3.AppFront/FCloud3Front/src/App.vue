<script setup lang="ts">
import Pop from './components/Pop.vue';
import TopbarParent from './components/Topbar/TopbarParent.vue';
import NeedMemberWarning from './components/NeedMemberWarning.vue';
import { useProvidesSetup } from './provides';

const { pop, displayTopbar, needMemberWarning} = useProvidesSetup();

</script>

<template>
  <Pop ref="pop"></Pop>
  <NeedMemberWarning ref="needMemberWarning"></NeedMemberWarning>
  <TopbarParent v-if="displayTopbar"></TopbarParent>
  <div class="main">
    <RouterView></RouterView>
  </div>
</template>

<style>
</style>