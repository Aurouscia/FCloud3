using FCloud3.Entities.Wiki;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Entities
{
    public interface IRelation
    {
        public int RelationMainId { get; }
        public int RelationSubId { get; }
        public int Order { get; set; }
    }

    public static class RelationListExtensions
    {
        public static void EnsureOrderDense<T>(this List<T> paras) where T:IRelation
        {
            paras.Sort((x, y) => x.Order - y.Order);
            paras.ResetOrder();
        }
        public static void ResetOrder<T>(this List<T> paras) where T:IRelation
        {
            for (int i = 0; i < paras.Count; i++)
                paras[i].Order = i;
        }
    }
}
