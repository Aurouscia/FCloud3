<script setup lang="ts">
import Pop from './components/Pop.vue';
import TopbarParent from './components/Topbar/TopbarParent.vue';
import { useProvidesSetup } from './provides';

const{pop, displayTopbar} = useProvidesSetup();

</script>

<template>
  <Pop ref="pop"></Pop>
  <TopbarParent v-if="displayTopbar"></TopbarParent>
  <div class="main">
    <RouterView></RouterView>
  </div>
</template>

<style>
</style>