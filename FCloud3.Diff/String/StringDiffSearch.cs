using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Diff.String
{
    public static class StringDiffSearch
    {
        public static StringDiffCollection Run(string? a, string? b, int alignThrs = 2)
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
                StringDiff diff;
                var diff1 = GenDiff(a, b, ptA, ptB, alignThrs, true);
                var diff2 = GenDiff(a, b, ptA, ptB, alignThrs, false);
                if (diff1.New + diff1.Ori.Length > diff2.New + diff2.Ori.Length)
                    diff = diff2;
                else
                    diff = diff1;
                diffs.Add(diff);
                ptA += diff.Ori.Length + alignThrs;
                ptB += diff.New + alignThrs;
            }
            return diffs;
        }

        private static StringDiff GenDiff(string a, string b, int ptA, int ptB, int alignThrs, bool flag)
        {
            int A_diffStart = ptA;
            int B_diffStart = ptB;
            int maxA = a.Length - 1;
            int maxB = b.Length - 1;
            if (flag)
            {
                while (true)
                {
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
                            return newDiff;
                        }
                        ptA++;
                    }

                    ptB++;
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
            else
            {
                while (true)
                {
                    ptB = B_diffStart;
                    while (true)
                    {
                        if (ptB > maxB)
                            break;
                        if (a[ptA] == b[ptB] && AlignConfirm(a, b, ptA, ptB, alignThrs))
                        {
                            int A_length = ptA - A_diffStart;
                            int B_length = ptB - B_diffStart;
                            StringDiff newDiff = new(
                                index: A_diffStart,
                                oriContent: a.Substring(A_diffStart, A_length),
                                newLength: B_length);
                            return newDiff;
                        }
                        ptB++;
                    }

                    ptA++;
                    if (ptA > maxA)
                    {
                        //一直到A的尽头也没有发现相同
                        int A_length = ptA - A_diffStart;
                        StringDiff newDiff = new(
                            index: A_diffStart,
                            oriContent: a.Substring(A_diffStart, A_length),
                            newLength: ptB - B_diffStart);
                        return newDiff;
                    }
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
