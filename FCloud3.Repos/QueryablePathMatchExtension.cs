using FCloud3.Entities;
using System.Linq.Expressions;
using System.Reflection;

namespace FCloud3.Repos
{
    public static class QueryablePathMatchExtension 
    {
        public static int GetIdByPath<T>(this IQueryable<T> q, string[] path) where T : IPathable
        {
            if (path.Length == 0)
                return 0;
            var possible = q.PathMatch(path).Select(x => new { x.Id, x.UrlPathName, x.Depth, x.ParentDir }).ToList();
            var currentParent = possible.FirstOrDefault(x => x.Depth == 0);
            if(currentParent is null) { return -1; }
            var k = 0;
            while (k++ < 15)
            {
                var child = possible.FirstOrDefault(x => x.ParentDir == currentParent.Id);
                if(child is null) 
                {
                    if (k == path.Length)
                        return currentParent.Id;
                    else
                        return -1;
                }
                currentParent = child;
            }
            return -1;
        }
        public static List<int>? GetChainIdsByPath<T>(this IQueryable<T> q, string[] path) where T : IPathable
        {
            if (path.Length == 0)
                return new();
            var possible = q.PathMatch(path).Select(x => new { x.Id, x.UrlPathName, x.Depth, x.ParentDir }).ToList();
            var currentParent = possible.FirstOrDefault(x => x.Depth == 0);
            if (currentParent is null) { return null; }

            var res = new List<int>();
            var k = 0;
            while (k++ < 15)
            {
                res.Add(currentParent.Id);
                var child = possible.FirstOrDefault(x => x.ParentDir == currentParent.Id);
                if (child is null)
                {
                    if (res.Count == path.Length)
                        return res;
                    else
                        return null;
                }
                currentParent = child;
            }
            return null;
        }
        public static List<T>? GetChainByPath<T>(this IQueryable<T> q, string[] path) where T : IPathable
        {
            if (path.Length == 0)
                return new();
            var possible = q.PathMatch(path).ToList();
            var currentParent = possible.FirstOrDefault(x => x.Depth == 0);
            if (currentParent is null) { return null; }

            var res = new List<T>();
            var k = 0;
            while (k++ < 15)
            {
                res.Add(currentParent);
                var child = possible.FirstOrDefault(x => x.ParentDir == currentParent.Id);
                if (child is null)
                {
                    if (res.Count == path.Length)
                        return res;
                    else
                        return null;
                }
                currentParent = child;
            }
            return null;
        }

        private static IQueryable<T> PathMatch<T>(this IQueryable<T> q, string[] path) where T : IPathable
        {
            Type t = typeof(T);
            PropertyInfo? nameProp = t.GetProperty(nameof(IPathable.UrlPathName));
            PropertyInfo? depthProp = t.GetProperty(nameof(IPathable.Depth));
            if(nameProp is null || depthProp is null) { throw new Exception("表达式构建出错(1)"); }

            ParameterExpression param = Expression.Parameter(t, "x");
            Expression? building = null;
            for (int i = 0; i < path.Length; i++) {
                string pathPart = path[i];

                MemberExpression nameMember = Expression.Property(param, nameProp);
                ConstantExpression nameShouldBe = Expression.Constant(pathPart);
                var eq1 = Expression.Equal(nameMember, nameShouldBe);

                MemberExpression depthMember = Expression.Property(param, depthProp);
                ConstantExpression depthShouldBe = Expression.Constant(i);
                var eq2 = Expression.Equal(depthMember, depthShouldBe);
                var cond = Expression.AndAlso(eq1, eq2);
                if (building is null)
                    building = cond;
                else
                    building = Expression.OrElse(building, cond);
            }
            if(building is null)
            {
                MemberExpression depthMember = Expression.Property(param, depthProp);
                ConstantExpression depthShouldBe = Expression.Constant(0);
                building = Expression.Equal(depthMember, depthShouldBe);
            }
            var lambda = Expression.Lambda(building, param) as Expression<Func<T,bool>>
                ?? throw new Exception("表达式构建出错(2)");
            return q.Where(lambda);
        }
    }
}
