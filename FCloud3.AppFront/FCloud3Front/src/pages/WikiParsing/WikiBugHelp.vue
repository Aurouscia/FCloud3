<script setup lang="ts">
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';
import { onMounted, onUnmounted } from 'vue';

onMounted(() => {
  setTitleTo('问题反馈')
})
onUnmounted(() => {
  recoverTitle()
})

const helpText = import.meta.env.VITE_WikiBugHelp || '请联系网站管理员'
</script>

<template>
  <h1>问题反馈</h1>
  <p>
    如果使用中遇到网站本体的问题，例如目录行为异常，或是语法的效果不符合“语法指南”中的说明：{{ helpText }}
  </p>
  <p>
    关于 script 标签：非常抱歉，由于无法消除的巨大安全隐患，script 标签不开放使用（会被自动去除）如果有特殊需要请考虑 <RouterLink :to="{name: 'wikiIframeWhitelist'}">iframe</RouterLink> 或 <RouterLink :to="{name:'wikiPlugins'}">新增插件</RouterLink>
  </p>
</template>

<style scoped lang="scss">
p{
  text-indent: 2em;
  margin-bottom: 1em;
  a{
    color: cornflowerblue;
  }
}
</style>
