

namespace FCloud3.Diff.String
{
    public class StringDiff(int index, string oriContent, int newLength)
    {
        public int Index { get; set; } = index;
        public string Ori { get; set; } = oriContent;
        public int New { get; set; } = newLength;

        public void Revert(List<char> current)
        {
            if (Ori.Length < New)
            {
                int needMoveIdx = Index + New;
                int needMoveDistance = New - Ori.Length;
                for(int i = needMoveIdx; i<current.Count; i++)
                {
                    current[i - needMoveDistance] = current[i];
                }
                current.RemoveRange(current.Count - needMoveDistance, needMoveDistance);
            }
            if(Ori.Length > New)
            {
                int needMoveIndex = Index + New;
                int needMoveDistance = Ori.Length - New;
                int countNow = current.Count;
                for (int i = needMoveDistance; i > 0; i--)
                {
                    current.Add(current[countNow - i]);
                }
                for (int i = needMoveIndex; i < countNow - needMoveDistance; i++) 
                {
                    current[i + needMoveDistance] = current[i];
                }
            }
            for(int i = 0; i < Ori.Length; i++)
            {
                current[Index + i] = Ori[i];
            }
        }
    }

    public class StringDiffCollection: List<StringDiff>
    {
        //public StringDiffCollection() { }

        //public string RevertAll(string newStr)
        //{
        //    this.Sort((x, y) => x.Index - y.Index);
        //    this.ForEach(x =>
        //    {
        //        x.Revert(newStr);
        //    });
        //}
    }
}
