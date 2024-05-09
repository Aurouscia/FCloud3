using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos
{
    public class IndexResult
    {
        public int PageIdx { get; set; }
        public int PageCount { get; set; }
        public int TotalCount { get; set; }
        public string[][] Data { get; set; }
        public IndexResult(int dataCount)
        {
            Data = new string[dataCount][];
        }
    }
    public class IndexResult<T> : IndexResult
    {
        public string[] ColumnNames { get; set; }
        public IndexResult(List<T> data, int pageIdx, int pageCount, int totalCount) : base(data.Count)
        { 
            PageIdx = pageIdx;
            PageCount = pageCount;
            TotalCount = totalCount;
            Type t = typeof(T);
            var props = t.GetProperties().ToList();
            int colCount = props.Count;
            ColumnNames = new string[colCount];

            for (var pIdx = 0; pIdx < colCount; pIdx++)
            {
                var p = props[pIdx];
                ColumnNames[pIdx] = p.Name;
            }

            for (var i = 0; i < data.Count; i++)
            {
                var row = new string[colCount];
                var item = data[i];
                for (var pIdx = 0; pIdx < colCount; pIdx++)
                {
                    var p = props[pIdx];
                    if (p.PropertyType.IsEnum)
                        row[pIdx] = ((int)p.GetValue(item)!).ToString();
                    else
                        row[pIdx] = p.GetValue(item)?.ToString() ?? "";
                }
                Data[i] = row;
            }
        }
    }
}
