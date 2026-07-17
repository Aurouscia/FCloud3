<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue';
import { setTitleTo, recoverTitle } from '@/utils/titleSetter';
import { injectPop } from '@/provides';
import copy from 'copy-to-clipboard';
import Footer from '@/components/Footer.vue';

onMounted(() => setTitleTo('自由分叉 / 私有化部署'));
onUnmounted(() => recoverTitle());

const dockerRun = ref('docker run -d -v fcloud3data:/app/Data -p 33442:8080 -p 33443:8081 fcloud3');
const copied = ref(false);

async function copyDockerCommand() {
    await navigator.clipboard.writeText(dockerRun.value);
    copied.value = true;
    setTimeout(() => copied.value = false, 1500);
}

const pop = injectPop();
const qq = '1783826971';
function copyQQ() {
    copy(qq);
    pop.value?.show('复制成功', 'success');
}

function scrollToQuickStart() {
    document.getElementById('quickstart')?.scrollIntoView({ behavior: 'smooth' });
}

const forkExamples = [
    {
        name: '平浪风云官网',
        subName: '定制化开发',
        url: 'http://maritel.jowei19.com',
        logo: 'http://maritel.jowei19.com/maritelLogo.png'
    },
    {
        name: '海岏世界观',
        subName: '原版使用',
        url: 'http://hwr.whjmctshb.com',
        logo: 'http://hwr.whjmctshb.com/hw.png'
    },
    {
        name: '玉河官网',
        subName: '原版使用',
        url: 'http://jukho.jowei19.com',
        logo: 'http://wiki.jowei19.com/fcloud.svg'
    }
]
</script>

<template>
    <div class="fork-page">
        <h1>Fork & 私有化部署</h1>
        <section class="hero">
            <img src="/fcloud.svg" alt="FCloud3" class="logo"/>
            <p class="tagline">
                想要一个<b>自主可控</b>的内容协作平台？<br/>
                那就创建一个自己的！
            </p>
            <p class="summary">
                FCloud3 是一个<b>开源项目</b>，以
                <a href="https://www.apache.org/licenses/LICENSE-2.0.html" target="_blank">Apache-2.0</a>
                协议提供。<br/>
                任何人有权下载、使用、修改、再分发本项目的源代码和可执行文件，且无需保持开源。
            </p>
            <div class="actions">
                <a class="cta" href="https://gitee.com/au114514/fcloud3" target="_blank">获取源码</a>
                <button class="cta secondary" @click="scrollToQuickStart">快速开始</button>
            </div>
        </section>

        <section class="section">
            <h2>开设私服，你将拥有</h2>
            <div class="cards">
                <div class="card">
                    <h3>专属协作空间</h3>
                    <p>你和你的创作团队可以获得一片不受打扰，自主可控的专属协作空间</p>
                </div>
                <div class="card">
                    <h3>自定义社区规则</h3>
                    <p>你可以为你的团队设立并执行自己的社区规则和创作规范</p>
                </div>
                <div class="card">
                    <h3>完全自由定制</h3>
                    <p>你可以随意修改系统的功能和外观（AI或手动），直到自己满意</p>
                </div>
            </div>
        </section>

        <section class="section">
            <h2>实际案例</h2>
            <div class="example-cards">
                <a v-for="e in forkExamples" :key="e.url" class="example-card" :href="e.url" target="_blank">
                    <img :src="e.logo" :alt="e.name" class="example-logo"/>
                    <div class="example-info">
                        <div class="example-name">{{ e.name }}</div>
                        <div class="example-sub">{{ e.subName }}</div>
                    </div>
                </a>
            </div>
        </section>

        <section class="section">
            <h2>你能用它做什么</h2>
            <div class="feature-grid">
                <div class="feature"><span class="icon">📝</span><h3>在线词条编辑</h3><p>支持 Markdown 与自动 Wiki 内链</p></div>
                <div class="feature"><span class="icon">📁</span><h3>目录与文件</h3><p>多层目录整理词条和图片、文档等附件</p></div>
                <div class="feature"><span class="icon">🔍</span><h3>版本历史 & Diff</h3><p>每次编辑都有记录，可逐行对比增删改</p></div>
                <div class="feature"><span class="icon">💬</span><h3>评论与互动</h3><p>词条下方可直接评论，适合讨论与反馈</p></div>
                <div class="feature"><span class="icon">🔐</span><h3>用户组权限</h3><p>按用户组控制编辑范围，实现团队协作</p></div>
                <div class="feature"><span class="icon">📊</span><h3>表格 & Excel</h3><p>内置表格编辑器，也支持导入导出 Excel</p></div>
                <div class="feature"><span class="icon">🖼️</span><h3>文件存储</h3><p>图片和文件可存在服务器本地或阿里云 OSS（可扩展）</p></div>
                <div class="feature"><span class="icon">🔌</span><h3>插件扩展</h3><p>可通过 FCloud3Plugins 扩展渲染与功能</p></div>
            </div>
        </section>

        <section class="section">
            <h2>公共实例 vs 自己部署</h2>
            <div class="table-wrap">
                <table class="compare-table">
                    <thead>
                        <tr><th>维度</th><th>使用公共实例</th><th>自己部署</th></tr>
                    </thead>
                    <tbody>
                        <tr><td>数据归属</td><td>×平台存储</td><td class="good">✔完全归你</td></tr>
                        <tr><td>社区规则</td><td>×受平台约束</td><td class="good">✔你说了算</td></tr>
                        <tr><td>品牌与域名</td><td>×平台域名</td><td class="good">✔你自己的域名和 Logo</td></tr>
                        <tr><td>功能与外观</td><td>×平台决定</td><td class="good">✔任意修改</td></tr>
                        <tr><td>商业化</td><td>×受限</td><td class="good">✔自由</td></tr>
                    </tbody>
                </table>
            </div>
        </section>

        <section class="section" id="quickstart">
            <h2>快速开始</h2>
            <p class="intro">
                FCloud3 支持 Docker（Linux）和 IIS（Windows）部署，你可以根据喜好自由选择。数据库默认使用 SQLite，无需额外安装配置，也可以选择使用任何 efcore 支持的数据库类型以获得更好性能。
                <a class="doc-link" href="https://gitee.com/au114514/fcloud3#%E5%AE%89%E8%A3%85" target="_blank">查看完整部署文档 →</a>
            </p>
            <div class="quickstart-cards">
                <div class="quickstart-card">
                    <h3>🐋 Linux / Docker</h3>
                    <p>构建镜像后，一行命令即可运行：</p>
                    <div class="code-block">
                        <code>{{ dockerRun }}</code>
                        <button class="copy" @click="copyDockerCommand">{{ copied ? '已复制' : '复制' }}</button>
                    </div>
                </div>
                <div class="quickstart-card">
                    <h3>🪟 Windows Server / IIS</h3>
                    <p>全程图形化操作</p>
                    <ol>
                        <li>安装 .NET 10 Hosting Bundle</li>
                        <li>在 IIS 中新建站点并指向程序目录</li>
                    </ol>
                </div>
            </div>
        </section>

        <section class="section">
            <h2>需要帮助？</h2>
            <div class="cards help-cards">
                <div class="card">
                    <h3>🎁免费借用服务器和子域名</h3>
                    <p>无法负担服务器开支？嫌麻烦？没关系，按照惯例，只要你的团队成员超过 <b>20人</b>，就可以免费借用我们的服务器和子级域名开设私服</p>
                    <p>你可以<b>长期借用</b>，等手头宽裕了可以<b>随时迁移</b>到自己的服务器和域名上。有意向者请联系 QQ：<button class="qq" @click="copyQQ">1783826971</button></p>
                </div>
                <div class="card">
                    <h3>🔧部署技术支持</h3>
                    <p>部署时遇到问题？请联系 QQ：<button class="qq" @click="copyQQ">1783826971</button>，我们会提供技术支持，或修复项目存在的问题</p>
                </div>
            </div>
        </section>

        <section class="section">
            <h2>系统亮点</h2>
            <div class="cards">
                <div class="card">
                    <h3>🏠跨平台</h3>
                    <p>服务端在 Linux 或 Windows 系统均可运行，也可以使用 OCI 容器托管服务</p>
                </div>
                <div class="card">
                    <h3>🔥高性能后端</h3>
                    <p>使用最新的 .NET 10 框架，关键功能经过高并发优化</p>
                </div>
                <div class="card">
                    <h3>⚡现代前端</h3>
                    <p>使用 Vue3 + TypeScript + Vite8，定制开发快捷且可靠</p>
                </div>
                <div class="card">
                    <h3>📊可缩放数据库</h3>
                    <p>可根据需求选择无需配置的 sqlite 或更强大的其他数据库（支持 mysql、mariadb、sqlserver、postgresql 等）</p>
                </div>
                <div class="card">
                    <h3>🧩可扩展语法引擎</h3>
                    <p>可根据需求添加自己的新语法，让你的创作团队快速实现想要的效果</p>
                </div>
                <div class="card">
                    <h3>🔧持续维护支持</h3>
                    <p>项目主仓库会不断添加新功能和 bug 修复，fork 仓库可选择随时同步补丁</p>
                </div>
            </div>
        </section>
    </div>
    <Footer></Footer>
</template>

<style scoped lang="scss">
.fork-page{
    max-width: 960px;
    margin: 0 auto;
    padding: 20px 0;
}
h1{
    text-align: center;
    font-size: 26px;
    margin-bottom: 12px;
    border: none;
}
.hero{
    background-color: #f9fafb;
    border-radius: 16px;
    padding: 32px 10px;
    text-align: center;
    margin-bottom: 32px;
    .logo{
        width: 96px;
        height: 96px;
        margin-bottom: 16px;
    }
    .tagline{
        font-size: 18px;
        color: #374151;
        line-height: 1.7;
        margin-bottom: 12px;
    }
    .summary{
        color: #555;
        line-height: 1.8;
        max-width: 640px;
        margin: 0 auto 20px;
    }
    .actions{
        display: flex;
        justify-content: center;
        flex-wrap: wrap;
        gap: 12px;
    }
    .cta{
        display: inline-flex;
        align-items: center;
        background-color: #2563eb;
        color: white;
        padding: 10px 24px;
        border-radius: 24px;
        text-decoration: none;
        font-weight: bold;
        border: none;
        cursor: pointer;
        transition: background-color 0.2s;
        &:hover{
            background-color: #1d4ed8;
        }
        &.secondary{
            background-color: white;
            color: #2563eb;
            box-shadow: inset 0 0 0 1px #2563eb;
            &:hover{
                background-color: #eff6ff;
            }
        }
    }
}
.section{
    margin-bottom: 36px;
    h2{
        font-size: 20px;
        margin-bottom: 16px;
        padding-left: 8px;
        border-left: 4px solid #2563eb;
    }
    .intro{
        color: #555;
        margin-bottom: 16px;
        line-height: 1.6em;
    }
}
.cards{
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
    gap: 16px;
}
.card{
    background-color: white;
    border: 1px solid #e5e7eb;
    border-radius: 12px;
    padding: 20px;
    box-shadow: 0 1px 3px rgba(0,0,0,0.04);
    transition: transform 0.15s, box-shadow 0.15s;
    &:hover{
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(0,0,0,0.08);
    }
    h3{
        font-size: 17px;
        margin-bottom: 8px;
        color: #111;
    }
    p{
        font-size: 14px;
        color: #666;
        line-height: 1.6;
        margin: 0 0 8px;
        &:last-child{
            margin-bottom: 0;
        }
    }
}
.help-cards{
    .qq{
        font-weight: 600;
        text-decoration: none;
        color: #2563eb;
        background: none;
        border: none;
        padding: 0;
        font: inherit;
        cursor: pointer;
        &:hover{
            text-decoration: underline;
        }
    }
}
.example-cards{
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
    gap: 16px;
}
.example-card{
    display: flex;
    align-items: center;
    gap: 12px;
    background-color: white;
    border: 1px solid #e5e7eb;
    border-radius: 12px;
    padding: 16px;
    text-decoration: none;
    transition: transform 0.15s, box-shadow 0.15s;
    &:hover{
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(0,0,0,0.08);
    }
    .example-logo{
        width: 48px;
        height: 48px;
        object-fit: contain;
        border-radius: 8px;
    }
    .example-info{
        .example-name{
            font-size: 16px;
            font-weight: 600;
            color: #111;
        }
        .example-sub{
            font-size: 13px;
            color: #666;
            margin-top: 4px;
        }
    }
}
.feature-grid{
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 14px;
}
.feature{
    background-color: white;
    border: 1px solid #e5e7eb;
    border-radius: 12px;
    padding: 16px;
    transition: transform 0.15s, box-shadow 0.15s;
    &:hover{
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(0,0,0,0.08);
    }
    .icon{
        display: block;
        font-size: 26px;
        margin-bottom: 8px;
    }
    h3{
        font-size: 16px;
        margin-bottom: 6px;
        color: #111;
    }
    p{
        color: #666;
        font-size: 14px;
        line-height: 1.5;
        margin: 0;
    }
}
.table-wrap{
    overflow-x: auto;
}
.compare-table{
    width: 100%;
    border-collapse: collapse;
    background-color: white;
    border: 1px solid #e5e7eb;
    border-radius: 12px;
    overflow: hidden;
    th, td{
        padding: 14px 16px;
        text-align: left;
        border-bottom: 1px solid #e5e7eb;
    }
    th{
        background-color: #f3f4f6;
        color: #374151;
        font-weight: 600;
    }
    td{
        color: #4b5563;
    }
    .good{
        color: #059669;
        font-weight: 600;
    }
}
.quickstart-cards{
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
    gap: 16px;
}
.quickstart-card{
    background-color: white;
    border: 1px solid #e5e7eb;
    border-radius: 12px;
    padding: 20px;
    h3{
        font-size: 17px;
        margin-bottom: 10px;
        color: #111;
    }
    p, ol{
        color: #555;
        line-height: 1.6;
        font-size: 14px;
    }
    ol{
        padding-left: 18px;
    }
    .hint{
        font-size: 13px;
        color: #6b7280;
        margin-top: 10px;
    }
    .doc-link{
        font-size: 14px;
        text-decoration: none;
        font-weight: 600;
    }
}
.code-block{
    display: flex;
    align-items: center;
    gap: 8px;
    background-color: #1f2937;
    color: #e5e7eb;
    border-radius: 8px;
    padding: 10px 12px;
    margin: 10px 0 0 0;
    code{
        flex: 1;
        font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
        font-size: 13px;
        word-break: break-all;
    }
    .copy{
        flex-shrink: 0;
        background-color: #374151;
        color: white;
        border: none;
        border-radius: 6px;
        padding: 6px 10px;
        font-size: 12px;
        cursor: pointer;
        transition: background-color 0.2s;
        &:hover{
            background-color: #4b5563;
        }
    }
}
a{
    color: #2563eb;
    text-decoration: underline;
}
</style>
