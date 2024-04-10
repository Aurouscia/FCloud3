using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Diff.String
{
    public static class StringDiffSearch
    {
        public static StringDiffCollection Run(string? a, string? b, int alignThrs = default)
        {
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
                StringDiff diff = GenDiff(a, b, ptA, ptB, alignThrs);
                diffs.Add(diff);
                ptA += diff.Ori.Length + alignThrs;
                ptB += diff.New + alignThrs;
            }
            return diffs;
        }

        private const int minThrs = 3;
        private static StringDiff GenDiff(string a, string b, int ptA, int ptB, int alignThrs)
        {
            int A_diffStart = ptA;
            int B_diffStart = ptB;
            int maxA = a.Length - 1;
            int maxB = b.Length - 1;
            int increA;
            int increB = 0;
            int thrs()
            {
                if (alignThrs != default)
                    return alignThrs;
                int thrs = minThrs;
                if (increA > thrs)
                    thrs = increA;
                if (increB > thrs)
                    thrs = increB;
                return thrs;
            }
            while (true)
            {
                ptA = A_diffStart;
                increA = 0;
                while (true)
                {
                    if (ptA > maxA)
                        break;
                    if (a[ptA] == b[ptB] && AlignConfirm(a, b, ptA, ptB, thrs()))
                    {
                        int A_length = ptA - A_diffStart;
                        int B_length = ptB - B_diffStart;
                        StringDiff newDiff = new(
                            index: A_diffStart,
                            oriContent: a.Substring(A_diffStart, A_length),
                            newLength: B_length);
                        return newDiff;
                    }
                    ptA++;
                    increA++;
                }

                ptB++;
                increB++;
                if (ptB > maxB)
                {
                    //一直到B的尽头也没有发现相同
                    int A_length = ptA - A_diffStart;
                    StringDiff newDiff = new(
                        index: A_diffStart,
                        oriContent: a.Substring(A_diffStart, A_length),
                        newLength: ptB - B_diffStart);
                    return newDiff;
                }
            }
        }

        private static bool AlignConfirm(string a, string b, int ptA, int ptB, int thrs)
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
    }
}
