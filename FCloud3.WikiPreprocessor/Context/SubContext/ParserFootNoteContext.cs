using FCloud3.WikiPreprocessor.Util;
using System.Text;

namespace FCloud3.WikiPreprocessor.Context.SubContext
{
    /// <summary>
    /// “脚注”上下文<br/>
    /// 用于收集解析中找到的脚注本体
    /// </summary>
    public class ParserFootNoteContext
    {
        public List<IHtmlable> FootNoteBodys { get; }
        public ParserFootNoteContext()
        {
            FootNoteBodys = new();
        }
        public void Clear()
        {
            FootNoteBodys.Clear();
        }
        public void AddFootNoteBody(IHtmlable body)
        {
            FootNoteBodys.Add(body);
        }
        public void AddFootNoteBodies(IEnumerable<IHtmlable?>? bodies)
        {
            if (bodies is not null)
                foreach (var b in bodies)
                {
                    if (b is not null)
                        AddFootNoteBody(b);
                }
        }
        public List<string> AllToString()
        {
            var sb = StringBuilderPool.Rent();
            var result = FootNoteBodys.ConvertAll(f =>
            {
                sb.Clear();
                f.WriteHtml(sb);
                return sb.ToString();
            });
            StringBuilderPool.Return(sb);
            return result;
        }
    }
}
