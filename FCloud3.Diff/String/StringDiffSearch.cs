using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Diff.String
{
    public static class StringDiffSearch
    {
        public static List<StringDiff> Run(string? a, string? b)
        {
            return Run(a, b, 1);
        }
        public static List<StringDiff> Run(string? a, string? b, int alignThrs)
        {
            a ??= "";
            b ??= "";
            List<StringDiff> diffs = [];
            if (a == b)
                return diffs;

            int ptA = 0, ptB = 0;
            int maxA = a.Length - 1;
            int maxB = b.Length - 1;
            while (true)
            {
                if (ptA > maxA && ptB > maxB)
                    break;
                if (ptA > maxA && ptB <= maxB)
                {
                    //说明B在A后面加了东西
                    StringDiff diffInline = new(ptA, "", b.Length - ptB);
                    diffs.Add(diffInline);
                    break;
                }
                if (ptA <= maxA && ptB > maxB)
                {
                    //说明B在A后面删了东西
                    StringDiff diffInline = new(ptA, a[ptA..], 0);
                    diffs.Add(diffInline);
                    break;
                }

                if (a[ptA] == b[ptB])
                {
                    ptA++; ptB++;
                    continue;
                }
                //发现不同之处
                int A_diffStart = ptA;
                int B_diffStart = ptB;
                while (true)
                {
                    bool ended = false;
                    ptA = A_diffStart;
                    while (true)
                    {
                        if (ptA > maxA)
                            break;
                        if (AlignConfirm(a, b, ptA, ptB, alignThrs))
                        {
                            int A_length = ptA - A_diffStart;
                            int B_length = ptB - B_diffStart;
                            StringDiff diffInline = new(
                                index: A_diffStart,
                                oriContent: a.Substring(A_diffStart, A_length),
                                newLength: B_length);

                            diffs.Add(diffInline);
                            ended = true;
                            break;
                        }
                        ptA++;
                    }
                    if (ended)
                        break;

                    ptB++;
                    if (ptB > maxB)
                    {
                        //一直到B的尽头也没有发现相同
                        StringDiff diffInline = new(
                            index: A_diffStart,
                            oriContent: a[A_diffStart..],
                            newLength: b.Length - B_diffStart);

                        diffs.Add(diffInline);
                        break;
                    }
                }
            }
            return diffs;
        }


        public static bool AlignConfirm(string a, string b, int ptA, int ptB, int thrs)
        {
            int aRemain = a.Length - ptA;
            int bRemain = b.Length - ptB;
            if (aRemain < thrs || bRemain < thrs)
                return false;
            for(int i = 0; i < thrs; i++)
            {
                char ca = a[ptA + i];
                char cb = b[ptB + i];
                if(ca != cb)
                    return false;
            }
            return true;
        }
    }
}
