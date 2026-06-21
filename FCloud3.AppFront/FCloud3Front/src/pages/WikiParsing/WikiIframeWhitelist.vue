<script setup lang="ts">
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';
import { onMounted, onUnmounted } from 'vue';

interface IframeSource {
  name: string
  origin: string
}

const rawWhitelist = (import.meta.env.VITE_IframeWhitelist as string | undefined) ?? '';
const applyText = (import.meta.env.VITE_IframeWhitelistApply as string | undefined) ?? '';

const iframeSources: IframeSource[] = rawWhitelist
  .split('\n')
  .map(line => line.trim())
  .filter(line => line.length > 0 && !line.startsWith('#'))
  .map(line => {
    const eqIdx = line.indexOf('=');
    if (eqIdx > 0) {
      return {
        name: line.substring(0, eqIdx).trim(),
        origin: line.substring(eqIdx + 1).trim()
      };
    }
    return { name: line, origin: line };
  })
  .filter(item => item.origin.startsWith('https://') || item.origin.startsWith('http://'));

onMounted(() => {
  setTitleTo('可用的 iframe 来源')
})
onUnmounted(() => {
  recoverTitle()
})
</script>

<template>
  <h1>可用的 iframe 来源</h1>
  <div>
    <p>iframe 允许在词条中嵌入第三方网页内容，但由于嵌入的页面可能包含脚本，存在以下潜在风险：</p>
    <ul>
      <li>嵌入页面可能包含恶意脚本，窃取用户信息或诱导点击</li>
      <li>嵌入页面可能展示与平台无关的广告或钓鱼内容</li>
      <li>嵌入页面可能通过追踪器收集用户行为数据</li>
    </ul>
    <p>因此，平台仅允许嵌入经过审核的可信域名。所有 iframe 都会被强制添加 sandbox 等安全属性，以最大程度降低风险。</p>
    <p v-if="applyText">如需申请添加新的 iframe 来源，{{ applyText }}。</p>
    <p v-else>暂不接收新增申请。</p>
  </div>
  <div v-if="iframeSources.length === 0" class="empty">暂无配置的 iframe 来源</div>
  <div v-else>
    <table>
      <thead>
        <tr>
          <th>名称</th>
          <th>域名</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="source in iframeSources" :key="source.origin">
          <td>{{ source.name }}</td>
          <td>{{ source.origin }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped lang="scss">
.empty {
  color: gray;
  text-align: center;
  padding: 40px 0;
  font-size: 16px;
}
ul{
  margin: 10px 20px;
}
</style>
