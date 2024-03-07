using FCloud3.HtmlGen.Util;
using MD5Hash;

namespace FCloud3.App.Services.Utils
{
    public class LocatorHash : ILocatorHash
    {
        public string? Hash(string? input)
        {
            return (input ?? "").GetMD5(EncodingType.UTF8);
        }
    }
}
