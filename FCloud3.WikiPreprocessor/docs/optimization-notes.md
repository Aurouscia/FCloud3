# 性能优化建议

> 基于对 FCloud3.WikiPreprocessor 项目的全面分析，整理可行的性能优化方向。
>
> 分析结论：**`Span<char>`/`ReadOnlyMemory<char>` 大范围替换 string 不可行**，原因见下文。推荐从其他方向入手。

---

## 一、为什么不推荐用 Span/Memory 替换 string

### 1.1 架构层面不可行

| 限制 | 说明 |
|------|------|
| `Span` 是 `ref struct` | 不能作为类字段、接口成员、集合元素 |
| 解析器核心全是接口+类 | `IHtmlable`、`IRule`、`IBlockRule`、`IInlineRule` 等 |
| 元素树需要长期存活 | `TextElement`、`Template`、`CachedElement` 持有文本 |
| 缓存系统依赖 string 哈希 | `Dictionary<int, CacheItem>` 以 `input.GetHashCode()` 为键 |

### 1.2 输入输出两端都是 string

```
输入 string ──→ 解析过程 ──→ 输出 string（ToHtml()）
     ↑                              ↓
  已经是 string              外部系统只接受 string
```

中间环节改为 `Span`/`Memory` 只是**延迟分配**，无法消除分配。

### 1.3 项目特征不适合

| 特征 | 说明 |
|------|------|
| 最大输入 30000 字符 | 子串分配成本本身不高 |
| 单次解析、单线程 | 没有大文本分块/异步管道场景 |
| 子串生命周期短 | 解析完即丢弃，不长期持有 |

### 1.4 ReadOnlyMemory<char> vs Span<char> 对比

| 特性 | `Span<char>` | `ReadOnlyMemory<char>` |
|------|-------------|------------------------|
| 可存字段 | ❌ | ✅ |
| 可入集合 | ❌ | ✅ |
| 可跨 async | ❌ | ✅ |
| 底层依赖 | 任意连续内存 | 必须依附托管对象 |
| 本项目收益 | 无（架构不兼容） | 极低（两端仍是 string） |

**结论**：`Memory` 突破了 `Span` 的"不能存字段"限制，但仍**没有解决根本问题**——输入输出都是 `string`，中间环节的 `Memory` 化增加复杂度却无实质收益。

---

## 二、推荐的优化方向（按性价比排序）

### 2.1 StringBuilder 复用（高优先级）

**问题**：`Parser` 中频繁创建 `StringBuilder`：

```csharp
// Parser.cs
StringBuilder resSb = new();           // RunToPlain
StringBuilder resSb = new();           // RunToParserResult
StringBuilder tempSb = new();          // ParserCacheContext
```

**建议**：引入 `StringBuilder` 池化（如 `ObjectPool<StringBuilder>` 或自定义池）。

```csharp
// 示例：自定义简单池
internal static class StringBuilderPool
{
    [ThreadStatic]
    private static StringBuilder? _cached;

    public static StringBuilder Rent()
    {
        var sb = _cached;
        if (sb is null) return new StringBuilder(1024);
        _cached = null;
        return sb;
    }

    public static void Return(StringBuilder sb)
    {
        sb.Clear();
        _cached = sb;
    }
}
```

**收益点**：
- `Parser.RunToPlain`
- `Parser.RunToParserResult`
- `Parser.RunToParserResultRaw`
- `ParserCacheContext.SaveParsedElement`
- `ParserFootNoteContext.AllToString`

---

### 2.2 List 预分配容量（中优先级）

**问题**：多处 `List<T>` 频繁扩容：

```csharp
// InlineParser.cs
InlineMarkList res = new();           // 默认容量 0，频繁扩容

// ElementCollection.cs
List<(bool isBlock, string body)> bodies = [];  // 同上

// BlockRule.cs - MiniTableBlockRule
List<string[]> text = lines.ToList().Select(...).ToList();  // 多次 ToList
```

**建议**：预估容量后预分配：

```csharp
// InlineParser.MakeMarks
InlineMarkList res = new();
// 改为：根据 input.Length 和规则复杂度预估
// 或直接用 List<InlineMark> 并预分配

// 更通用的：ElementCollection 添加带容量的构造函数
public ElementCollection(int capacity) : base(capacity) { }
```

---

### 2.3 减少不必要的字符串操作（中优先级）

#### 2.3.1 合并 Substring + Trim

```csharp
// PrefixBlockRule.cs - 当前
public override string GetPureContentOf(string line)
{
    if (LineMatched(line))
        return line.Substring(Mark.Length).Trim();  // 先 Substring 再 Trim
    return line;
}

// 优化：用 Span 做中间处理，只分配一次结果
public override string GetPureContentOf(string line)
{
    if (LineMatched(line))
        return line.AsSpan(Mark.Length).Trim().ToString();  // 零中间分配
    return line;
}
```

#### 2.3.2 延迟 ToString

```csharp
// HtmlArea.cs - 当前
string tagName;
if (matched[1] == '/') 
    tagName = matched[2..^1];  // 分配 string
else
{
    int firstSpace = matched.IndexOf(' ');
    if (firstSpace == -1)
        tagName = matched[1..^1];  // 又分配
    else
        tagName = matched[1..firstSpace];  // 再分配
}
tagName = tagName.ToLower();  // 再分配

// 优化：只在最后转 string
ReadOnlySpan<char> matchedSpan = matched.AsSpan();
ReadOnlySpan<char> tagNameSpan;
if (matchedSpan[1] == '/')
    tagNameSpan = matchedSpan[2..^1];
else
{
    int firstSpace = matchedSpan.IndexOf(' ');
    tagNameSpan = firstSpace == -1 
        ? matchedSpan[2..^1] 
        : matchedSpan[1..firstSpace];
}
string tagName = tagNameSpan.ToString().ToLower();
```

#### 2.3.3 模板插槽解析

```csharp
// TemplateElement.cs - PlainSlot 构造函数
public PlainSlot(string value, int start) : base(value, value[3..^3], start) { }
// value[3..^3] 产生中间 string

// 优化
public PlainSlot(string value, int start) 
    : base(value, value.AsSpan()[3..^3].ToString(), start) { }
```

---

### 2.4 正则表达式优化（中优先级）

**问题**：部分正则无预编译或每次调用重新编译：

```csharp
// Escape.cs
[GeneratedRegex(@"\\(?=(\[|\]|\*|\||-|~|#))")]
private static partial Regex RemoveEscapeChar();

// 这是 GeneratedRegex（.NET 7+），已预编译，没问题

// FootNoteBodyRule.cs
[GeneratedRegex(@"^\s*\[\^.{1,}\][:：]{1}")]
private static partial Regex FootBodyLineMarkRegex();

// 也是 GeneratedRegex，没问题
```

**潜在问题**：`ColorTextRule.IsColorTextAtLineStart` 中：

```csharp
var m = Regex.Match(line, "(?<=^#).{2,}?(?=#)");
```

这是**非预编译正则**，每次调用都解析正则语法。

**建议**：改为 `GeneratedRegex` 或静态 `Regex` 实例：

```csharp
[GeneratedRegex("(?<=^#).{2,}?(?=#)")]
private static partial Regex ColorTextLineRegex();
```

---

### 2.5 缓存键冲突优化（低优先级）

**问题**：`ParserCacheContext` 使用 `input.GetHashCode()` 作为键：

```csharp
private static int CacheKey(string input) => input.GetHashCode();
```

**风险**：不同输入可能哈希冲突，导致缓存命中错误。

**建议**：使用更可靠的键，如 `string.GetHashCode(StringComparison.Ordinal)` 的确定性版本，或直接用 `string` 做键（牺牲一点内存，换取正确性）：

```csharp
private readonly Dictionary<string, CacheItem> _cacheDict;
// 而非 Dictionary<int, CacheItem>
```

> 注：当前 30000 字符上限下，哈希冲突概率极低，此优化优先级不高。

---

### 2.6 减少委托和 Lambda 分配（低优先级）

**问题**：部分 LINQ 和 Lambda 在热路径中：

```csharp
// BlockParser.cs
var pureLines = generating.Select(x => x.PureContent).ToList();

// MiniTableBlockRule.cs
List<string[]> text = lines.ToList().Select(x=>x.Text).Select(x => x.Split(tableSep)).ToList();
int width = text.Select(x => x.Length).Max();
int headSepRowIndex = text.FindIndex(x => x.All(y => y.Length>=3 && y.All(c => c == '-')));
```

**建议**：热路径中避免 LINQ，改为手写循环：

```csharp
// 示例：替代 Select + ToList
var pureLines = new List<LineAndHash>(generating.Count);
foreach (var g in generating)
    pureLines.Add(g.PureContent);
```

---

## 三、优化优先级总览

| 优先级 | 优化项 | 预估收益 | 改动复杂度 |
|--------|--------|---------|-----------|
| 🔴 高 | StringBuilder 池化 | 高（减少大量小对象分配） | 低 |
| 🟡 中 | List 预分配容量 | 中（减少数组扩容） | 低 |
| 🟡 中 | 合并 Substring+Trim（Span 局部使用） | 中（减少中间 string） | 低 |
| 🟡 中 | 正则预编译 | 中（减少解析开销） | 低 |
| 🟢 低 | 缓存键改为 string | 低（提升正确性） | 低 |
| 🟢 低 | 热路径去 LINQ | 低（减少委托分配） | 中 |
| ❌ 不推荐 | Span/Memory 大范围替换 string | 理论高，实际无 | 极高 |

---

## 四、Span/Memory 的合理使用场景

虽然**大范围替换不可行**，但在以下局部场景可以安全使用 `Span`：

```csharp
// ✅ 纯本地计算，不存储结果
public string GetPureContentOf(string line)
{
    return line.AsSpan(Mark.Length).Trim().ToString();  // 零中间分配
}

// ✅ 字符串比较和搜索
ReadOnlySpan<char> span = input.AsSpan();
int idx = span.IndexOf("target");

// ✅ 提取子串后立刻消费
foreach (var part in input.AsSpan().Split('|'))  // .NET 无内置 Split(Span)，需手写
{
    Process(part.Trim());
}

// ❌ 不能存储到字段
public class TextElement
{
    public ReadOnlySpan<char> Content;  // 编译错误！
}

// ❌ 不能作为接口成员
public interface IRule
{
    ReadOnlySpan<char> GetName();  // 编译错误！
}
```

---

## 五、总结

> **不要为了追求"零分配"而引入不必要的复杂度。**
>
> 本项目的瓶颈不在 `Substring` 分配，而在：
> 1. **频繁的 StringBuilder 创建**
> 2. **List 扩容带来的数组分配**
> 3. **不必要的中间 string（Substring + Trim 链）**
>
> 优先解决这些问题，收益更明确，代码更易维护。
