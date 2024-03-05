using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Util;
using FCloud3.App.Utils;
using Microsoft.Extensions.Caching.Memory;

namespace FCloud3.App.Services.Utils
{
    public class LocatorHash : ILocatorHash
    {
        //TODO：去掉Trim()
        public string? Hash(string? input)
        {
            return MD5Helper.GetMD5Of($"locHash_{input?.Trim()}");
        }
    }
}
