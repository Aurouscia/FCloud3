using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FCloud3.Entities.Wiki;

namespace FCloud3.App.Utils;
// 片段结构
public readonly struct SpanSegment(string text, bool isChinese)
{
    public string Text { get; } = text;
    public bool IsChinese { get; } = isChinese;
}

public static class PinYinHelper
{
    public static string ToUrlName(string input)
    {
        var segs = SplitToSegments(input);
        var resList = new List<string>();
        foreach(var s in segs)
        {
            if (s.IsChinese)
                resList.AddRange(EzPinyin.PinyinHelper.GetArray(s.Text));
            else
            {
                string cleaned = Regex.Replace(s.Text, @"[^A-Za-z0-9-]", "");
                resList.Add(cleaned);
            }
        }
        string res;
        if (resList.Sum(c => c.Length) + resList.Count > WikiItem.urlPathNameMaxLength)
        {
            var firstLetters = resList
                .Where(x => x.Length>0)
                .Select(c => c[0])
                .ToArray();
            res = new string(firstLetters);
        }
        else
        {
            res = string.Join('-', resList);
        }
        // 1. 去掉首尾连字符，并把中间连续的多个连字符压缩成单个
        res = Regex.Replace(res.Trim('-'), @"-+", "-");

        // 2. 再次兜底去掉可能残留的收尾连字符
        res = Regex.Replace(res, @"^-+|-+$", "");
        return res;
    }
    
    // 判断 32 位 Unicode 标量是否落在 CJK 表意文字区
    private static bool IsChineseRune(int scalar)
    {
        uint v = (uint)scalar;
        return (v >= 0x4E00  && v <= 0x9FFF) ||
               (v >= 0x3400  && v <= 0x4DBF) ||
               (v >= 0x20000 && v <= 0x2CEAF);
    }

    // 从指定索引开始拆出一个 Rune，返回标量值 & 消耗字符数
    private static bool TryReadRune(string s, int index, out int scalar, out int consumed)
    {
        consumed = 0;
        scalar   = 0;
        if ((uint)index >= (uint)s.Length) return false;

        char c1 = s[index];
        consumed = 1;

        if (!char.IsHighSurrogate(c1))
        {
            scalar = c1;
            return true;
        }

        // 高代理，必须还有低代理
        if (index + 1 < s.Length && char.IsLowSurrogate(s[index + 1]))
        {
            scalar   = char.ConvertToUtf32(c1, s[index + 1]);
            consumed = 2;
            return true;
        }
        // 畸形代理，按无效字符处理（这里直接当成普通字符）
        scalar = c1;
        return true;
    }

    private static List<SpanSegment> SplitToSegments(string input)
    {
        if (string.IsNullOrEmpty(input)) return [];

        var list   = new List<SpanSegment>();
        int  i     = 0;
        int  n     = input.Length;

        while (i < n)
        {
            if (!TryReadRune(input, i, out int firstScalar, out int firstConsumed))
                break;

            bool curIsCh = IsChineseRune(firstScalar);
            int  start   = i;
            i += firstConsumed;

            // 继续扫同类型
            while (i < n)
            {
                if (!TryReadRune(input, i, out int nextScalar, out int nextConsumed))
                    break;

                if (IsChineseRune(nextScalar) != curIsCh)
                    break;

                i += nextConsumed;
            }

            string piece = input.Substring(start, i - start);
            list.Add(new SpanSegment(piece, curIsCh));
        }

        return list;
    }
}