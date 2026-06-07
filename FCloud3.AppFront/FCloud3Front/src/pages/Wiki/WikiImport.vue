<script setup lang="ts">
import { ref } from 'vue';
import { ImportPreview } from '@/models/etc/wikiImportExport';
import { injectApi, injectPop } from '@/provides';

const api = injectApi();
const pop = injectPop();
const selectedFile = ref<File | null>(null);
const preview = ref<ImportPreview | null>(null);
const checkingFiles = ref(false);
const importing = ref(false);

async function onFileChange(e: Event) {
    const target = e.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {
        selectedFile.value = target.files[0];
        preview.value = null;
        await loadPreview();
    }
}

async function loadPreview() {
    if (!selectedFile.value) return;
    const data = await api.split.wikiImportExport.previewImport(selectedFile.value);
    if (data) {
        preview.value = data;
    }
}

async function checkFileStatus() {
    if (!preview.value || preview.value.Files.length === 0) return;
    checkingFiles.value = true;
    const urls = preview.value.Files.map(f => f.FullUrl);
    const results = await api.split.wikiImportExport.checkFileStatus(urls);
    if (results) {
        for (const file of preview.value.Files) {
            const result = results.find(r => r.Url === file.FullUrl);
            if (result) {
                file.Accessible = result.Accessible;
                file.Size = result.Size;
            }
        }
    }
    checkingFiles.value = false;
}

function formatBytes(bytes?: number): string {
    if (bytes === undefined || bytes === null) return '未知';
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
}

async function doImport() {
    if (!selectedFile.value) return;
    importing.value = true;
    const resp = await api.split.wikiImportExport.importWikis(selectedFile.value);
    importing.value = false;
    if (resp) {
        pop.value.show(`成功导入 ${resp.ImportedCount} 个词条`, 'success');
        preview.value = null;
        selectedFile.value = null;
    }
}

function reset() {
    selectedFile.value = null;
    preview.value = null;
}
</script>

<template>
    <div>
        <h1>词条导入</h1>
        <div class="section">
            <input type="file" accept=".zip" @change="onFileChange" v-if="!selectedFile"/>
            <div v-else class="fileInfo">
                <span>{{ selectedFile.name }}</span>
                <button @click="reset">重新选择</button>
            </div>
        </div>

        <div v-if="preview" class="previewSection">
            <h2>词条列表（共 {{ preview.Wikis.length }} 个）</h2>
            <table class="previewTable index">
                <thead>
                    <tr>
                        <th>标题</th>
                        <th>原路径名</th>
                        <th>导入后路径名</th>
                        <th>冲突</th>
                        <th>段落类型</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="wiki in preview.Wikis" :key="wiki.OriginalUrlPathName" :class="{conflict: wiki.HasConflict}">
                        <td>{{ wiki.Title }}</td>
                        <td>{{ wiki.OriginalUrlPathName }}</td>
                        <td>{{ wiki.ResolvedUrlPathName }}</td>
                        <td>
                            <span v-if="wiki.HasConflict" class="conflictBadge">冲突</span>
                            <span v-else class="okBadge">正常</span>
                        </td>
                        <td>{{ wiki.ParaTypes.join('、') }}</td>
                    </tr>
                </tbody>
            </table>

            <h2>引用文件（共 {{ preview.Files.length }} 个）</h2>
            <div v-if="preview.Files.length === 0" class="empty">无文件引用</div>
            <div v-else>
                <button @click="checkFileStatus" :disabled="checkingFiles">
                    {{ checkingFiles ? '检查中...' : '检查文件状态' }}
                </button>
                <table class="previewTable index">
                    <thead>
                        <tr>
                            <th>文件名</th>
                            <th>路径</th>
                            <th>状态</th>
                            <th>大小</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="file in preview.Files" :key="file.StorePathName">
                            <td>{{ file.DisplayName }}</td>
                            <td class="pathCell">{{ file.StorePathName }}</td>
                            <td>
                                <span v-if="file.Accessible === undefined">未检查</span>
                                <span v-else-if="file.Accessible" class="okBadge">可访问</span>
                                <span v-else class="errorBadge">不可访问</span>
                            </td>
                            <td>{{ formatBytes(file.Size) }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <div class="importActions">
                <button @click="doImport" :disabled="importing" class="importBtn">
                    {{ importing ? '导入中...' : '确认导入' }}
                </button>
            </div>
        </div>
    </div>
</template>

<style scoped lang="scss">
.fileInfo {
    display: flex;
    align-items: center;
    gap: 12px;
}
.previewSection {
    margin-top: 20px;
}
.previewSection h2 {
    margin-top: 20px;
    margin-bottom: 10px;
    font-size: 16px;
}
.previewTable {
    width: 100%;
    border-collapse: collapse;
    font-size: 13px;
}
.previewTable th,
.previewTable td {
    text-align: left;
}
.previewTable tr.conflict {
    background: #fff3cd;
}
.previewTable td:nth-child(4) {
    white-space: nowrap;
}
.conflictBadge {
    background: #dc3545;
    color: white;
    padding: 2px 8px;
    border-radius: 4px;
    font-size: 12px;
}
.okBadge {
    background: #28a745;
    color: white;
    padding: 2px 8px;
    border-radius: 4px;
    font-size: 12px;
}
.errorBadge {
    background: #dc3545;
    color: white;
    padding: 2px 8px;
    border-radius: 4px;
    font-size: 12px;
}
.empty {
    color: #999;
    padding: 12px 0;
}
.pathCell {
    max-width: 300px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}
.importActions {
    margin-top: 24px;
    margin-bottom: 100px;
    text-align: center;
}
</style>
