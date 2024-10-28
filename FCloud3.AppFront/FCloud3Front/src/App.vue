<script setup lang="ts">
import Pop from './components/Pop.vue';
import TopbarParent from './components/Topbar/TopbarParent.vue';
import NeedMemberWarning from './components/NeedMemberWarning.vue';
import { useProvidesSetup } from './provides';
import Wait from './components/Wait.vue';
import { useMainDivDisplayStore } from './utils/globalStores/mainDivDisplay';
import { storeToRefs } from 'pinia';

const { pop, displayTopbar, needMemberWarning, wait } = useProvidesSetup();
const { restrictContentMaxWidth, displayMarginTop, enforceScrollY } = storeToRefs(useMainDivDisplayStore())
</script>

<template>
  <Pop ref="pop"></Pop>
  <Wait ref="wait"></Wait>
  <NeedMemberWarning ref="needMemberWarning"></NeedMemberWarning>
  <TopbarParent v-if="displayTopbar"></TopbarParent>
  <div class="main" :class="{displayMarginTop, enforceScrollY}">
    <div class="mainInner" :class="{restrictContentMaxWidth}">
      <RouterView></RouterView>
    </div>
  </div>
</template>

<style scoped lang="scss">
@use './styles/globalValues';

.displayMarginTop{
  margin-top: globalValues.$topbar-height;
  height: calc(100vh - globalValues.$topbar-height);
}
.restrictContentMaxWidth{
  padding: 0px 20px 0px 20px;
  max-width: 1200px;
}
.enforceScrollY{
  overflow-y: scroll;
}
</style>