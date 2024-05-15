using System.Text;

namespace FCloud3.WikiPreprocessor.Context.SubContext
{
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
            StringBuilder sb = new();
            return FootNoteBodys.ConvertAll(f =>
            {
                sb.Clear();
                f.WriteHtml(sb);
                return sb.ToString();
            });
        }
    }
}
