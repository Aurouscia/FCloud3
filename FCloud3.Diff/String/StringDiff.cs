

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
                for (int i = countNow - needMoveDistance - 1; i >= needMoveIndex; i--)
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
        public string RevertAll(string newStr)
        {
            var charList = newStr.ToList();
            this.Sort((x, y) => x.Index - y.Index);
            this.ForEach(x =>
            {
                x.Revert(charList);
            });
            return new string(charList.ToArray());
        }

        public void RevertAll(List<char> str)
        {
            this.Sort((x, y) => x.Index - y.Index);
            this.ForEach(x =>
            {
                x.Revert(str);
            });
        }

        public int AddedChars() => this.Select(x => x.New).Sum();
        public int RemovedChars() => this.Select(x => x.Ori.Length).Sum();
    }
}
