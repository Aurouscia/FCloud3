<script setup lang="ts">
import Pop from './components/Pop.vue';
import Topbar from './components/Topbar.vue';
import { useProvidesSetup } from './provides';

const{pop, displayTopbar} = useProvidesSetup();

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