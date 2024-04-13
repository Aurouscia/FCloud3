

namespace FCloud3.Diff.Display
{
    public class CutSpan
    {
        public int Index { get; set; }
        public int End { get; set; }
        public int Length => End - Index;
        public List<int[]> Highlights { get; set; } = [];
        public void AddHighlight(int from, int to)
        {
            Highlights.Add([from - Index, to - Index]);
        }
        public static List<CutSpan> Make(List<int[]> from, int totalLength, int thickness)
        {
            from.Sort((x, y) => x[0] - y[0]);
            List<CutSpan> res = [];
            for(int i = 0; i < from.Count; i++)
            {
                int[] target = from[i];
                var myExpectedStart = target[0] - thickness;
                if (myExpectedStart < 0)
                    myExpectedStart = 0;
                int myExpectedEnd = target[1] + thickness;
                if (myExpectedEnd > totalLength)
                    myExpectedEnd = totalLength; //end指的是空隙的位置，可以是最后一个字符之后
                var last = res.LastOrDefault();
                if (last is null || last.End < myExpectedStart) 
                {
                    CutSpan newSpan = new()
                    {
                        Index = myExpectedStart,
                        End = myExpectedEnd
                    };
                    newSpan.AddHighlight(target[0], target[1]);
                    res.Add(newSpan);
                }
                else
                {
                    last.AddHighlight(target[0], target[1]);
                    last.End = myExpectedEnd;
                }
            }
            return res;
        }
    }
}
