using System.Text.RegularExpressions;

namespace FCloud3.WikiPreprocessor.Util
{
    public partial class HtmlArea
    {
        public static List<Range> FindRanges(string input)
        {
            var tags = HtmlTags().Matches(input);
            Stack<string> tagStack = new();
            List<Range> result = [];
            int startingPointer = 0;
            int lastHeight = 0;
            foreach(var match in tags.AsEnumerable())
            {
                string matched = match.Value;
                string tagName;
                bool isFrontTag = true;
                if (matched[1] == '/') 
                {
                    tagName = matched[2..^1]; 
                    isFrontTag = false;
                }
                else
                {
                    int firstSpace = matched.IndexOf(' ');
                    if (firstSpace == -1)
                        tagName = matched[1..^1];
                    else
                        tagName = matched[1..firstSpace];
                }
                tagName = tagName.ToLower();

                if (isFrontTag)
                {
                    //是左标签，标签名入栈
                    tagStack.Push(tagName);
                }
                else
                {
                    //是右标签，不停地出栈到找到匹配的左标签为止
                    while (true)
                    {
                        if (tagStack.Count == 0)
                            break;
                        var poped = tagStack.Pop();
                        if (poped == tagName)
                            break;
                    }
                }

                if (lastHeight == 0 && tagStack.Count > 0)
                {
                    //进入html区域，记录起始点
                    startingPointer = match.Index; 
                }
                else if (lastHeight > 0 && tagStack.Count == 0)
                {
                    //离开html区域，记录Range
                    result.Add(new Range(startingPointer, match.Index + match.Length - 1));
                    startingPointer = 0;
                }
                lastHeight = tagStack.Count;
            }
            if(startingPointer != 0)
            {
                //格式异常，标签未闭合，直接取最后一个标签当结尾
                var lastMatch = tags.Last();
                result.Add(new Range(startingPointer, lastMatch.Index + lastMatch.Length - 1));
            }
            return result;
        }

        [GeneratedRegex("</?[A-Za-z]+?([ \n]+?[^ <>]*?)*?>", RegexOptions.None, 100)]
        private static partial Regex HtmlTags();
    }
}
