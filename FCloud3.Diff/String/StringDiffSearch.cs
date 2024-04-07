using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Diff.String
{
    public static class StringDiffSearch
    {
        public static StringDiffCollection Run(string? a, string? b, int alignThrs = -1)
        {
            if (alignThrs == -1)
                AutoAlighThrs(a, b);
            a ??= "";
            b ??= "";
            StringDiffCollection diffs = [];
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
                    StringDiff newDiff = new(ptA, "", b.Length - ptB);
                    diffs.Add(newDiff);
                    break;
                }
                if (ptA <= maxA && ptB > maxB)
                {
                    //说明B在A后面删了东西
                    StringDiff newDiff = new(ptA, a[ptA..], 0);
                    diffs.Add(newDiff);
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
                        if (a[ptA] == b[ptB] && AlignConfirm(a, b, ptA, ptB, alignThrs))
                        {
                            int A_length = ptA - A_diffStart;
                            int B_length = ptB - B_diffStart;
                            StringDiff newDiff = new(
                                index: A_diffStart,
                                oriContent: a.Substring(A_diffStart, A_length),
                                newLength: B_length);

                            diffs.Add(newDiff);
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
                        int A_length = ptA - A_diffStart;
                        StringDiff newDiff = new(
                            index: A_diffStart,
                            oriContent: a.Substring(A_diffStart, A_length),
                            newLength: ptB - B_diffStart);

                        diffs.Add(newDiff);
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
            if (aRemain == bRemain && bRemain < thrs)
                thrs = bRemain;
            else if (aRemain < thrs || bRemain < thrs)
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

        public static int AutoAlighThrs(string? a, string? b)
        {
            int lengthA = a is null ? 0 : a.Length;
            int lengthB = b is null ? 0 : b.Length;
            int smaller = Math.Min(lengthA, lengthB);
            int res = smaller / 10;
            return Math.Max(res, 10);
        }
    }
}
