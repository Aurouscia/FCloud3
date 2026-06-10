<script setup lang="ts">
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';
import { onMounted, onUnmounted } from 'vue';
import pluginsFound from '@/build/plugin/pluginsFound.json'

interface PluginInfo {
  name: string
  entry: string
  triggers: string[]
  priority: number
  docs?: string
}

const plugins = pluginsFound as PluginInfo[]

onMounted(() => {
  setTitleTo('Wiki插件')
})
onUnmounted(() => {
  recoverTitle()
})
</script>

<template>
  <h1>Wiki插件</h1>
  <div v-if="plugins.length === 0" class="empty">暂无可用插件</div>
  <div v-else class="wikiPlugins">
    <div v-for="plugin in plugins" :key="plugin.name" class="pluginItem">
      <h2>{{ plugin.name }}</h2>
      <div class="triggers">
        <span class="label">触发词：</span>
        <code v-for="trigger in plugin.triggers" :key="trigger" class="trigger">{{ trigger }}</code>
      </div>
      <div v-if="plugin.docs" class="docs">
        <a :href="plugin.docs" target="_blank">
          <button class="ok">查看文档</button>
        </a>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.empty {
  color: gray;
  text-align: center;
  padding: 40px 0;
  font-size: 16px;
}
.wikiPlugins {
  display: flex;
  flex-direction: column;
  align-items: stretch;
  gap: 20px;
}
.pluginItem {
  display: flex;
  flex-direction: column;
  gap: 8px;
  border-bottom: 1px solid gray;
  padding-bottom: 20px;
  margin-bottom: 20px;

  h2 {
    font-size: 20px;
    margin: 0;
  }

  .label {
    color: gray;
    font-size: 14px;
  }

  .triggers {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 6px;

    code.trigger {
      background: #f4f4f4;
      padding: 2px 8px;
      border-radius: 4px;
      font-family: monospace;
      color: #c41d7f;
      font-weight: bold;
    }
  }

  .entry code {
    background: #f8f8f8;
    padding: 2px 6px;
    border-radius: 3px;
    font-family: monospace;
    font-size: 12px;
    color: #666;
  }

  .docs {
    margin-top: 4px;

    a {
      color: cornflowerblue;
      font-weight: bold;
    }
  }
}
</style>
