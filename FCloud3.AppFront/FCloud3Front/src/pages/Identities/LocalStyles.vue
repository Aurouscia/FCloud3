<script setup lang="ts">
import { useLocalStylesStore } from '@/utils/globalStores/localStyles';
import { storeToRefs } from 'pinia';

const { 
    css,
    fontSizeRem,
    lineHeightRem,
    color,
    fontFamily
} = storeToRefs(useLocalStylesStore())
</script>

<template>
<div>
    <h1>自定义样式</h1>
    <div class="note">仅在本设备保存和生效</div>
    <table style="width: 100%;"><tbody>
        <tr>
            <td>文本<br/>大小</td>
            <td>
                {{ fontSizeRem || '默认值' }}<br/>
                <input v-model.number="fontSizeRem" type="range" min="0" max="3" step="0.1"/>
            </td>
        </tr>
        <tr>
            <td>文本<br/>行高</td>
            <td>
                {{ lineHeightRem || '默认值' }}<br/>
                <input v-model.number="lineHeightRem" type="range" min="0" max="6" step="0.1"/>
            </td>
        </tr>
        <tr>
            <td>文本<br/>颜色</td>
            <td>
                <input v-model="color" placeholder="井号开头颜色代码"/>
                <div class="note">可填任意css合法颜色表达式，如</div>
                <div class="note">#ffeecc/rgb(172, 168, 1)/white</div>
            </td>
        </tr>
        <tr>
            <td>字体</td>
            <td>
                <input v-model="fontFamily" placeholder="字体名称"/>
                <div class="note">可填多个逗号隔开的字体名</div>
                <div class="note">衬线:serif&nbsp;无衬线:sans-serif</div>
                <div class="note">若设备有字体设置，可能无效</div>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <textarea v-model="css" placeholder="自定义CSS代码" rows="5" spellcheck="false"></textarea>
                <div class="note">
                    css选择器防污染建议<br/>
                    限定词条页内范围 .wikiView<br/>
                    限定词条段落内范围 .indent<br/>
                    例如 .wikiView p { ... }
                </div>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <div class="note">
                    此处改动均自动保存<br/>但刷新后才能看到效果
                </div>
            </td>
        </tr>
    </tbody></table>
</div>
</template>

<style lang="scss" scoped>
.note{
    font-size: 14px;
    color: #999;
}
input{
    width: 140px;
}
textarea{
    width: 230px;
    font-family: unset;
    font-size: unset;
    padding: 5px;
}
</style>