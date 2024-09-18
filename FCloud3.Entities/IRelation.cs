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
        public int Id { get; set; }
        public int RelationMainId { get; }
        public int RelationSubId { get; }
        public int Order { get; set; }
    }

    public static class RelationListExtensions
    {
        public static void EnsureOrderDense<T>(this List<T> items) where T:class, IRelation
        {
            items.Sort((x, y) => x.Order - y.Order);
            items.ResetOrder();
        }
        public static void ResetOrder<T>(this List<T> items) where T:class, IRelation
        {
            for (int i = 0; i < items.Count; i++)
                items[i].Order = i;
        }
        public static void ResetOrder<T>(this List<T> items, List<int> ids) where T :class, IRelation
        {
            items.Sort((x, y) =>
            {
                return ids.IndexOf(x.Id) - ids.IndexOf(y.Id);
            });
            items.ResetOrder();
        }
    }
}
