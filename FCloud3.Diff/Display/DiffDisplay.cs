using FCloud3.Diff.String;

namespace FCloud3.Diff.Display
{
    /// <summary>
    /// 用于展示某次更改具体位置的数据
    /// </summary>
    public class DiffDisplay
    {
        /// <summary>
        /// 表示较旧版本的文本段，其中的高亮应该是被移除部分，建议涂为红色
        /// </summary>
        public List<DiffDisplayFrag> From { get; set; } = [];
        /// <summary>
        /// 表示较新版本的文本段，其中的高亮应该是被添加部分，建议涂为绿色
        /// </summary>
        public List<DiffDisplayFrag> To { get; set; } = [];

        public static DiffDisplay Make(List<char> content, StringDiffCollection diffs, int thickness)
        {
            var res = new DiffDisplay();
            thickness = Math.Clamp(thickness, 0, 20);
            List<int[]> addedSpans = [];
            List<int[]> removedSpans = [];
            int offset = 0;
            diffs.Sort((x, y) => x.Index - y.Index);
            diffs.ForEach(x => {
                int addedIndex = x.Index + offset;
                addedSpans.Add([addedIndex, addedIndex + x.New]);
                offset += x.New - x.Ori.Length;

                int oriLength = x.Ori is not null ? x.Ori.Length : 0;
                if (oriLength > 0)
                    removedSpans.Add([x.Index, x.Index + oriLength]);
            });
            var added = CutSpan.Make(addedSpans, content.Count, thickness);
            added.ForEach(x => res.To.Add(new(x, content)));
            diffs.RevertAll(content);
            var removed = CutSpan.Make(removedSpans, content.Count, thickness);
            removed.ForEach(x => res.From.Add(new(x, content)));
            return res;
        }
    }
    /// <summary>
    /// 表示一个被截取的文本片段，其中有些指定要高亮的区域，索引相对于自己的开头
    /// </summary>
    public class DiffDisplayFrag
    {
        public DiffDisplayFrag(string content, List<int[]> highlights)
        {
            Content = content;
            Highlights = highlights;
        }
        public DiffDisplayFrag(CutSpan cutSpan, List<char> content)
        {
            var span = new char[cutSpan.Length];
            content.CopyTo(cutSpan.Index, span, 0, cutSpan.Length);
            Content = new(span);
            Highlights = cutSpan.Highlights;
        }
        public string Content { get; set; }
        public List<int[]> Highlights { get; set; }
    }
}
