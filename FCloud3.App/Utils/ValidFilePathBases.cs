using Microsoft.AspNetCore.Mvc.Routing;
using System.Globalization;

namespace FCloud3.App.Utils
{
    public static class ValidFilePathBases
    {
        private readonly static List<string> validPaths = new List<string> { upload, wikiFile, material, forum, test };
        public const string upload = "upload";
        public const string wikiFile = "wikiFile";
        public const string material = "material";
        public const string forum = "forum";
        public const string test = "test";
        public static bool Contains(string path)
        {
            return validPaths.Contains(path);
        }
    }

    public class FilePathBaseConstraint : IRouteConstraint
    {
        public const string constraintName = "filePathBase";
        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (values.TryGetValue(routeKey, out object? routeValue))
            {
                var str = routeValue?.ToString();
                if(str is not null)
                {
                    return ValidFilePathBases.Contains(str);
                }
            }
            return false;
        }
    }

    public static class FilePathBaseConstraintConfigure
    {
        public static IServiceCollection AddFilePathBaseConstraint(this IServiceCollection services) 
        {
            services.Configure<RouteOptions>(x => x.ConstraintMap.Add(FilePathBaseConstraint.constraintName, typeof(FilePathBaseConstraint)));
            return services;
        }
    }
}
