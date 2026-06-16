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
  displayName?: string
  description?: string
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
  <h1>可用的插件列表</h1>
  <div v-if="plugins.length === 0" class="empty">暂无可用插件</div>
  <div v-else class="wikiPlugins">
    <div class="table-notice">
      <p>插件一般要求一个表格，对于较小的表格，你可以使用：</p>
      <div class="table-example">
        | 第一行第一列 | 第一行第二列 |<br/>| 第二行第一列 | 第二行第二列 |
      </div> 
      <div class="table-example">
        | 一行一列的表格 |
      </div> 
      <p>来在文本段落内创建小表格，注意表格语法需要单独起一行，行内除了竖线和单元格内容外，不能有其他东西。</p>
      <p>对于较大的表格，请创建表格段落，以方便编辑。</p>
      <p>想添加功能？对现有功能不满意？如果你有计算机基础且对创建插件感兴趣，可以在源代码的 FCloud3.AppFront\FCloud3Plugins 目录照葫芦画瓢创建新插件并在 gitee 上提 pr</p>
    </div>
    <div v-for="plugin in plugins" :key="plugin.name" class="pluginItem">
      <h2>{{ plugin.displayName || plugin.name }}</h2>
      <div v-if="plugin.description" class="description">{{ plugin.description }}</div>
      <div class="triggers">
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
.table-notice{
  p{
    margin-top: 0.5em;
    text-indent: 2em;
  }
  .table-example{
    background-color: #eee;
    padding: 5px;
    border-radius: 5px;
    width: fit-content;
    margin: 10px 0px;
  }
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
  border-top: 1px solid gray;
  padding-top: 20px;
  margin-bottom: 20px;

  h2 {
    font-size: 20px;
    margin: 0;
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

  .description {
    color: #666;
    font-size: 14px;
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
